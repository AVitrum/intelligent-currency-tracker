using System.Globalization;
using Application.Common.Exceptions;
using Domain.Constans;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Infrastructure.ExternalApis.PrivateBank;

public class ExchangeRateFetcherService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExchangeRateFetcherService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer;

    public ExchangeRateFetcherService(
        IHttpClientFactory httpClientFactory,
        ILogger<ExchangeRateFetcherService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(CheckAndFetchData, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private async void CheckAndFetchData(object? state)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IAppSettings appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
        IExchangeRateRepository repository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
        IExchangeRateFactory factory = scope.ServiceProvider.GetRequiredService<IExchangeRateFactory>();

        DateTime lastDate = await repository.GetLastDateAsync();
        DateTime firstToFetch = lastDate.AddDays(1);
        DateTime today = DateTime.UtcNow.Date;

        for (DateTime date = firstToFetch; date <= today; date = date.AddDays(1))
        {
            var dateString = date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            await ProcessDateAsync(appSettings.PrivateBankUrl + "/exchange_rates?json&date={0}", dateString, repository, factory, CancellationToken.None);
        }
    }

    private async Task ProcessDateAsync(string urlTemplate, string date, IExchangeRateRepository repository,  IExchangeRateFactory factory, CancellationToken stoppingToken)
    {
        var url = string.Format(urlTemplate, date);
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                bool exists = await repository.ExistsByDateAsync(
                    DateTime.SpecifyKind(
                        DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture),
                        DateTimeKind.Utc).ToUniversalTime());

                if (exists)
                {
                    _logger.LogWarning("Data for {Date} already exists", date);
                    break;
                }

                await FetchAndSaveDataAsync(url, date, repository, factory, stoppingToken);
                _logger.LogInformation("Successfully fetched data for {Date}", date);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data for {Date}, attempt {Attempt}", date, attempt + 1);
                if (attempt == 2) _logger.LogError("Max retries reached for {Date}", date);
                else await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task FetchAndSaveDataAsync(string url, string date, IExchangeRateRepository repository, IExchangeRateFactory factory, CancellationToken token)
    {
        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync(url, token);

            response.EnsureSuccessStatusCode();

            string jsonData = await response.Content.ReadAsStringAsync(token);
            var jsonObject = JObject.Parse(jsonData);

            string responseDate = jsonObject["date"]?.ToString() ?? throw new Exception("Missing 'date' field in response");
            var parsedDate = DateTime.ParseExact(responseDate, DateConstants.DateFormat, CultureInfo.InvariantCulture);
            var parsedDateString = parsedDate.Date.ToString(CultureInfo.InvariantCulture);

            if (parsedDate != DateTime.ParseExact(date, DateConstants.DateFormat, CultureInfo.InvariantCulture))
            {
                throw new WrongDateException(date, parsedDateString);
            }

            JToken? exchangeRatesToken = jsonObject["exchangeRate"];
            if (exchangeRatesToken is not { HasValues: true })
            {
                throw new Exception("Missing or empty 'exchangeRate' field in response");
            }

            foreach (JToken rateToken in exchangeRatesToken)
            {
                await factory.CreateExchangeRate(rateToken);
            }
        }
        catch (WrongDateException e)
        {
            _logger.LogError(e.Message);
        }
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}