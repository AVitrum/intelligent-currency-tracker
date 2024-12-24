using System.Net.Http.Headers;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.ExchangeRates.Results;
using Domain.Common;
using Microsoft.Extensions.Logging;

namespace Application.MlModel;

public class MlModelService : IMlModelService
{
    private readonly IAppSettings _appSettings;
    private readonly ILogger<MlModelService> _logger;
    private readonly ICsvHelper _csvHelper;
    private readonly HttpClient _httpClient;

    public MlModelService(IAppSettings appSettings, ILogger<MlModelService> logger, ICsvHelper csvHelper, HttpClient httpClient)
    {
        _appSettings = appSettings;
        _logger = logger;
        _csvHelper = csvHelper;
        _httpClient = httpClient;
    }

    public async Task<BaseResult> TrainModelAsync(ExchangeRatesRangeDto dto)
    {
        _logger.LogInformation("Starting TrainModelAsync");

        BaseResult baseResult = await _csvHelper.ExportExchangeRateToCsvAsync(dto);

        if (baseResult is not ExportExchangeRatesToCsvResult exportResult)
        {
            _logger.LogWarning("Failed to export exchange rates to CSV");
            return BaseResult.FailureResult(["Failed to export exchange rates to CSV"]);
        }

        _logger.LogInformation("Successfully exported exchange rates to CSV");

        using var content = new MultipartFormDataContent();
        using var fileContentStream = new ByteArrayContent(exportResult.FileContent);
        fileContentStream.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        content.Add(fileContentStream, "file", exportResult.FileName);

        _logger.LogInformation("Sending POST request to {Url}", $"{_appSettings.ModelUrl}/train-model");

        HttpResponseMessage response = await _httpClient.PostAsync($"{_appSettings.ModelUrl}/train-model", content);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Model training started successfully");
            return BaseResult.SuccessResult();
        }

        var errorMessage = await response.Content.ReadAsStringAsync();
        _logger.LogError("Failed to start model training: {ErrorMessage}", errorMessage);
        return BaseResult.FailureResult([errorMessage]);
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
            var prediction = await response.Content.ReadAsStringAsync();
            return ExchangeRatePredictionResult.SuccessResult(prediction);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get prediction: {Message}", ex.Message);
            return BaseResult.FailureResult(["Failed to get prediction. Please try again later."]);
        }
    }
}