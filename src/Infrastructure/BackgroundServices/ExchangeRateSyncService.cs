using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;
using Policy = Polly.Policy;

namespace Infrastructure.BackgroundServices;

public class ExchangeRateSyncService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExchangeRateSyncService> _logger;

    private readonly List<int> _r030 = [];
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _updateInterval;

    public ExchangeRateSyncService(
        IHttpClientFactory httpClientFactory,
        ILogger<ExchangeRateSyncService> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _updateInterval =
            TimeSpan.Parse(configuration.GetValue<string>("ExchangeRateSync:UpdateInterval") ?? "01:00:00");
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public event ExchangeRatesFetchedEventHandler? ExchangeRatesFetched;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_updateInterval);

        do
        {
            await FetchAndStoreExchangeRates(stoppingToken);

            if (_r030.Count != 0 && ExchangeRatesFetched != null)
            {
                await ExchangeRatesFetched.Invoke(this, new ExchangeRatesFetchedEventArgs(_r030));
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task FetchAndStoreExchangeRates(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IAppSettings appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
            IRateRepository rateRepository = scope.ServiceProvider.GetRequiredService<IRateRepository>();

            DateTime lastProcessedDate = await rateRepository.GetLastDateAsync();
            DateTime startDate = lastProcessedDate.AddDays(1);
            DateTime currentDate = DateTime.UtcNow.Date;

            if (startDate > currentDate)
            {
                _logger.LogInformation("No new rates to fetch.");
                return;
            }

            HttpClient client = _httpClientFactory.CreateClient();

            const int batchSize = 10;
            List<DateTime> datesToProcess = Enumerable
                .Range(0, (currentDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .ToList();

            foreach (DateTime[] batch in datesToProcess.Chunk(batchSize))
            {
                IEnumerable<Task> tasks = batch.Select(async date =>
                {
                    using IServiceScope innerScope = _scopeFactory.CreateScope();
                    IRateRepository rateRepo = innerScope.ServiceProvider.GetRequiredService<IRateRepository>();
                    ICurrencyRepository currencyRepo =
                        innerScope.ServiceProvider.GetRequiredService<ICurrencyRepository>();

                    await HandleApiResponseAsync(client, BuildNbuApiUrl(appSettings.NbuUrl, date),
                        date, rateRepo, currencyRepo, stoppingToken);
                });

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching exchange rates.");
        }
    }

    private async Task HandleApiResponseAsync(
        HttpClient client,
        string url,
        DateTime date,
        IRateRepository rateRepository,
        ICurrencyRepository currencyRepository,
        CancellationToken stoppingToken)
    {
        AsyncRetryPolicy? policy = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        await policy.ExecuteAsync(async () =>
        {
            HttpResponseMessage response = await client.GetAsync(url, stoppingToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            JArray ratesData = JArray.Parse(responseBody);

            ICollection<Rate> rates = await ParseExchangeRatesAsync(ratesData, date, currencyRepository);
            rates = await CompareToPreviousAsync(rates, rateRepository);

            await rateRepository.AddRangeAsync(rates);

            _logger.LogInformation("Exchange rates for {Date} were successfully stored.", date);
        });
    }

    private async Task<ICollection<Rate>> ParseExchangeRatesAsync(
        JArray ratesData,
        DateTime date,
        ICurrencyRepository currencyRepository)
    {
        List<Rate> rates = [];

        foreach (JToken rateToken in ratesData)
        {
            string? currencyCode = rateToken["cc"]?.Value<string>();

            if (string.IsNullOrEmpty(currencyCode))
            {
                _logger.LogWarning("Currency code is missing in the response for {Date}.", date);
                continue;
            }

            Currency currency = await EnsureCurrencyExistsAsync(currencyCode, rateToken, currencyRepository);

            decimal rateValue = rateToken["rate"]?.Value<decimal>() ?? 0;
            rates.Add(new Rate
            {
                CurrencyId = currency.Id,
                Value = rateValue,
                Date = date
            });

            if (!_r030.Contains(currency.R030))
            {
                _r030.Add(currency.R030);
            }
        }

        return rates;
    }

    private static string BuildNbuApiUrl(string baseUrl, DateTime date)
    {
        string formattedDate = date.ToString("yyyyMMdd");
        return $"{baseUrl}/NBUStatService/v1/statdirectory/exchange?date={formattedDate}&json";
    }

    private static async Task<ICollection<Rate>> CompareToPreviousAsync(
        ICollection<Rate> rates,
        IRateRepository rateRepository)
    {
        foreach (Rate rate in rates)
        {
            Rate lastRate = await rateRepository.GetLastByCurrencyIdAsync(rate.CurrencyId);
            rate.ValueCompareToPrevious = rate.Value - lastRate.Value;
        }

        return rates;
    }

    private static async Task<Currency> EnsureCurrencyExistsAsync(
        string currencyCode,
        JToken rateToken,
        ICurrencyRepository currencyRepository)
    {
        Currency? currency = await currencyRepository.GetByCodeAsync(currencyCode);
        if (currency is not null)
        {
            return currency;
        }

        string currencyName = rateToken["txt"]?.Value<string>() ?? "Unknown";
        int r030 = rateToken["r030"]?.Value<int>() ?? 0;

        currency = new Currency
        {
            Code = currencyCode,
            Name = currencyName,
            R030 = r030
        };

        await currencyRepository.AddAsync(currency);
        return currency;
    }
}