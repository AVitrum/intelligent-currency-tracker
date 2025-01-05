using System.Text;
using System.Text.Json;
using Application.Common.Interfaces;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Application.ExchangeRates.Results;
using Confluent.Kafka;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.MlModel;

//TODO: Change service when finished fixing model.
public class MlModelService : IMlModelService
{
    private readonly IAppSettings _appSettings;
    private readonly ILogger<MlModelService> _logger;
    private readonly ICsvHelper _csvHelper;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly HttpClient _httpClient;
    
    public MlModelService(
        IAppSettings appSettings,
        ILogger<MlModelService> logger,
        ICsvHelper csvHelper,
        IKafkaProducer kafkaProducer,
        HttpClient httpClient)
    {
        _appSettings = appSettings;
        _logger = logger;
        _csvHelper = csvHelper;
        _kafkaProducer = kafkaProducer;
        _httpClient = httpClient;
    }

    public async Task<BaseResult> TrainModelAsync(ExchangeRateRequest request)
    {
        (_, byte[] content) = await _csvHelper.ExportExchangeRateToCsvAsync(request);
        
        string csvContent = Encoding.UTF8.GetString(content);
        var message = new { content = csvContent };
        string serializedMessage = JsonSerializer.Serialize(message);
        int messageSize = Encoding.UTF8.GetByteCount(serializedMessage);
        
        _logger.LogInformation("Message size: {Size} bytes", messageSize);
        
        await _kafkaProducer.ProduceAsync("train-model",
            new Message<string, string> { Value = serializedMessage });
        
        _logger.LogInformation("Model training started successfully");
        return BaseResult.SuccessResult();
    }

    public async Task<BaseResult> PredictAsync(ExchangeRatePredictionDto dto)
    {
        var url = $"{_appSettings.ModelUrl}/predict/?pre_date={dto.PreDate}&currency_code={dto.CurrencyCode}";
        _logger.LogInformation("Sending GET request to {Url}", url);

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Successfully received prediction");
            string prediction = await response.Content.ReadAsStringAsync();
            
            return ExchangeRatePredictionResult.SuccessResult(prediction);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get prediction: {Message}", ex.Message);
            return BaseResult.FailureResult(["Failed to get prediction. Please try again later."]);
        }
    }
}