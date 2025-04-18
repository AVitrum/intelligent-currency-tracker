using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.AiModel.Results;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Application.ExchangeRates.Results;
using Domain.Common;

namespace Application.AiModel;

public class AiModelService : IAiModelService
{
    private readonly IAppSettings _appSettings;
    private readonly ICsvService _csvService;
    private readonly IRateRepository _rateRepository;
    private readonly HttpClient _httpClient;

    public AiModelService(IAppSettings appSettings, IHttpClientFactory httpClientFactory, ICsvService csvService, IRateRepository rateRepository)
    {
        _appSettings = appSettings;
        _csvService = csvService;
        _rateRepository = rateRepository;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<BaseResult> TrainModelAsync(int currencyR030)
    {
        try
        {
            MultipartFormDataContent content = await CreateMultipartContentAsync(currencyR030);
            HttpResponseMessage response = await SendTrainRequestAsync(content);
            return await ProcessTrainResponseAsync(response);
        }
        catch (Exception ex)
        {
            return BaseResult.FailureResult(new List<string> { ex.Message });
        }
    }

    private async Task<MultipartFormDataContent> CreateMultipartContentAsync(int currencyR030)
    {
        MultipartFormDataContent multiFormContent = new MultipartFormDataContent();

        multiFormContent.Add(new StringContent(currencyR030.ToString()), "currency_r030");

        DateTime lastDate = await _rateRepository.GetLastDateAsync();
        BaseResult exportResult = await _csvService.ExportExchangeRatesToCsvAsync(
            DateTime.Parse("01.01.2014").ToUniversalTime(), lastDate, currencyR030);

        if (!exportResult.Success)
        {
            throw new Exception("Failed to generate CSV file.");
        }

        if (exportResult is not ExportExchangeRatesToCsvResult csvResult)
        {
            throw new Exception("Invalid CSV file format.");
        }

        ByteArrayContent fileContent = new ByteArrayContent(csvResult.FileContent);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        multiFormContent.Add(fileContent, "file", csvResult.FileName);

        return multiFormContent;
    }

    private async Task<HttpResponseMessage> SendTrainRequestAsync(MultipartFormDataContent content)
    {
        return await _httpClient.PostAsync($"{_appSettings.ModelUrl}/train", content);
    }

    private async Task<BaseResult> ProcessTrainResponseAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.OK)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            return TrainResult.SuccessResult(responseContent);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            return BaseResult.FailureResult(new List<string> { errorContent });
        }

        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            return BaseResult.FailureResult(["Model API is not available."]);
        }

        return BaseResult.FailureResult(new List<string> { "Failed to train the model." });
    }


    public async Task<BaseResult> PredictAsync(int currencyR030, string date)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"{_appSettings.ModelUrl}/predict",
            JsonContent.Create(new { currency_r030 = currencyR030, date }));

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            BaseResult trainResult = await TrainModelAsync(currencyR030);
            
            if (trainResult.Success)
            { 
                return await PredictAsync(currencyR030, date);
            }
            
            return BaseResult.FailureResult(["Model not trained."]);
        }

        if (response.StatusCode == HttpStatusCode.OK)
        {
            string content = await response.Content.ReadAsStringAsync();
            PredictionResponse? deserialize = JsonSerializer.Deserialize<PredictionResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (deserialize is not null)
            {
                return PredictResult.SuccessResult(deserialize);
            }
        }

        return BaseResult.FailureResult(["Failed to predict the value."]);
    }
}