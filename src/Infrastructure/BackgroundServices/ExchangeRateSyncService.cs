using System.Net;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Infrastructure.BackgroundServices;

public class ExchangeRateSyncService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExchangeRateSyncService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExchangeRateSyncService(
        IHttpClientFactory httpClientFactory,
        ILogger<ExchangeRateSyncService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromDays(1));

        do
        {
            await FetchAndStoreExchangeRates(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task FetchAndStoreExchangeRates(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IAppSettings appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
            IRateRepository rateRepository = scope.ServiceProvider.GetRequiredService<IRateRepository>();
            ICurrencyRepository currencyRepository = scope.ServiceProvider.GetRequiredService<ICurrencyRepository>();

            DateTime lastProcessedDate = await rateRepository.GetLastDateAsync();
            DateTime startDate = lastProcessedDate.AddDays(1);
            DateTime currentDate = DateTime.UtcNow.Date;

            if (startDate > currentDate)
            {
                _logger.LogInformation("No new rates to fetch.");
                return;
            }

            HttpClient client = _httpClientFactory.CreateClient();

            for (DateTime date = startDate; date <= currentDate; date = date.AddDays(1))
            {
                string url = BuildNbuApiUrl(appSettings.NbuUrl, date);
                await HandleApiResponseAsync(client, url, date, rateRepository, currencyRepository, stoppingToken);
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
        try
        {
            HttpResponseMessage response = await client.GetAsync(url, stoppingToken);

            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                _logger.LogWarning("Received 504 Gateway Timeout. Retrying for {Date}", date);
                await HandleApiResponseAsync(client, url, date, rateRepository, currencyRepository, stoppingToken);
                return;
            }

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
            JArray ratesData = JArray.Parse(responseBody);

            ICollection<Rate> rates = await ParseExchangeRatesAsync(ratesData, date, currencyRepository);
            rates = await CompareToPreviousAsync(rates, rateRepository);
            
            await rateRepository.AddRangeAsync(rates);

            _logger.LogInformation("Exchange rates for {Date} were successfully stored.", date);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing API response for {Date}", date);
        }
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