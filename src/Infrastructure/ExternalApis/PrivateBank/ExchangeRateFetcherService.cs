using System.Globalization;
using System.Text.Json;
using Confluent.Kafka;
using Domain.Constans;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Infrastructure.ExternalApis.PrivateBank;

public class ExchangeRateFetcherService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ExchangeRateFetcherService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public ExchangeRateFetcherService(IHttpClientFactory httpClientFactory, ILogger<ExchangeRateFetcherService> logger, IServiceProvider serviceProvider)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => _ = ConsumeAsync("fetch-exchange-rates", stoppingToken), stoppingToken);
    }

    private async Task ConsumeAsync(string topic, CancellationToken stoppingToken)
    {
        string kafkaHost;
        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            var appSettings = scope.ServiceProvider.GetRequiredService<IAppSettings>();
            kafkaHost = appSettings.KafkaHost;
        }
        
        var config = new ConsumerConfig
        {
            GroupId = "fetch-exchange-rates-group",
            BootstrapServers = kafkaHost,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            MaxPollIntervalMs = 600000
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                var requestData = JsonSerializer.Deserialize<RequestData>(consumeResult.Message.Value);

                if (requestData == null)
                {
                    _logger.LogError("Received null or invalid data from Kafka");
                    continue;
                }

                HttpClient httpClient = _httpClientFactory.CreateClient();
                using IServiceScope scope = _serviceProvider.CreateScope();
                var exchangeRateRepository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();

                foreach (var date in requestData.Dates)
                {
                    var formattedUrl = string.Format(requestData.UrlTemplate, date);
                    for (var attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            _logger.LogInformation("Fetching data for {Date}, attempt {Attempt}", date,
                                attempt + 1);
                            HttpResponseMessage response = await httpClient.GetAsync(formattedUrl, stoppingToken);

                            response.EnsureSuccessStatusCode();
                            var jsonData = await response.Content.ReadAsStringAsync(stoppingToken);
                            JToken exchangeRates = JObject.Parse(jsonData)["exchangeRate"]!;

                            var exchangeRate = exchangeRates.Select(rate => new ExchangeRate
                            {
                                Date = DateTime.ParseExact((string)JObject.Parse(jsonData)["date"]!,
                                    DateConstants.DateFormat,
                                    CultureInfo.InvariantCulture),
                                Currency = Enum.Parse<Currency>((string)rate["currency"]!),
                                SaleRateNb = (decimal?)rate["saleRateNB"] ?? 0,
                                PurchaseRateNb = (decimal?)rate["purchaseRateNB"] ?? 0,
                                SaleRate = (decimal?)rate["saleRate"] ?? 0,
                                PurchaseRate = (decimal?)rate["purchaseRate"] ?? 0
                            });
                            await exchangeRateRepository.SaveExchangeRatesAsync(exchangeRate.ToList());

                            _logger.LogInformation("Successfully fetched data for {Date}", date);
                            break;
                        }
                        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23505" }) 
                        {
                            _logger.LogWarning("Data for {Date} already exists in the database", date);
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to fetch data for {Date}, attempt {Attempt}", date,
                                attempt + 1);
                            if (attempt == 2)
                            {
                                _logger.LogError("Max retries reached for {Date}", date);
                            }
                            else
                            {
                                await Task.Delay(1000, stoppingToken);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming messages from Kafka");
            }
        }

        consumer.Close();
    }

    private record RequestData
    {
        public List<string> Dates { get; init; } = null!;
        public string UrlTemplate { get; init; } = null!;
    }
}