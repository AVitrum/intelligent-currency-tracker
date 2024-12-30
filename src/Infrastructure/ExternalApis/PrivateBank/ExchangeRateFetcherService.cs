using System.Globalization;
using System.Text.Json;
using Confluent.Kafka;
using Domain.Constans;
using Domain.Enums;
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
    
    public ExchangeRateFetcherService(
        IHttpClientFactory httpClientFactory,
        ILogger<ExchangeRateFetcherService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(() => ConsumeAsync("fetch-exchange-rates", stoppingToken), stoppingToken);

    private async Task ConsumeAsync(string topic, CancellationToken stoppingToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IAppSettings appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
        string kafkaHost = appSettings.KafkaHost;
        
        var config = new ConsumerConfig
        {
            GroupId = "fetch-exchange-rates-group",
            BootstrapServers = kafkaHost,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            MaxPollIntervalMs = 600000
        };

        using IConsumer<string, string> consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string> consumeResult = consumer.Consume(stoppingToken);
                RequestData? requestData = JsonSerializer.Deserialize<RequestData>(consumeResult.Message.Value);

                if (requestData == null)
                {
                    _logger.LogError("Received invalid data from Kafka");
                    continue;
                }

                foreach (string date in requestData.Dates)
                {
                    await ProcessDateAsync(requestData.UrlTemplate, date, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consuming Kafka messages");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessDateAsync(string urlTemplate, string date, CancellationToken stoppingToken)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IExchangeRateRepository repository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();

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

                await FetchAndSaveDataAsync(url, repository, stoppingToken);
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

    private async Task FetchAndSaveDataAsync(string url, IExchangeRateRepository repository, CancellationToken token)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient();
        HttpResponseMessage response = await httpClient.GetAsync(url, token);
        
        response.EnsureSuccessStatusCode();
        
        string jsonData = await response.Content.ReadAsStringAsync(token);
        var jsonObject = JObject.Parse(jsonData);
        
        string responseDate = jsonObject["date"]?.ToString() ?? throw new Exception("Missing 'date' field in response");
        var parsedDate = DateTime.ParseExact(responseDate, DateConstants.DateFormat, CultureInfo.InvariantCulture);

        JToken? exchangeRatesToken = jsonObject["exchangeRate"];
        if (exchangeRatesToken is not { HasValues: true })
        {
            throw new Exception("Missing or empty 'exchangeRate' field in response");
        }

        var exchangeRates = exchangeRatesToken.Select(rate => new ExchangeRate
        {
            Date = parsedDate,
            Currency = Enum.TryParse(rate["currency"]?.ToString(), out Currency currency) ? currency : throw new Exception("Invalid 'currency' field"),
            SaleRateNb = rate["saleRateNB"]?.ToObject<decimal>() ?? 0,
            PurchaseRateNb = rate["purchaseRateNB"]?.ToObject<decimal>() ?? 0,
            SaleRate = rate["saleRate"]?.ToObject<decimal>() ?? 0,
            PurchaseRate = rate["purchaseRate"]?.ToObject<decimal>() ?? 0
        }).ToList();

        await repository.SaveExchangeRatesAsync(exchangeRates);
    }

    private record RequestData
    {
        public List<string> Dates { get; init; } = null!;
        public string UrlTemplate { get; init; } = null!;
    }
}