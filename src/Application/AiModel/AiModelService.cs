using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.AiModel.Results;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
using Domain.Common;

namespace Application.AiModel;

public class AiModelService : IAiModelService
{
    private readonly IAppSettings _appSettings;
    private readonly HttpClient _httpClient;

    public AiModelService(IAppSettings appSettings, IHttpClientFactory httpClientFactory)
    {
        _appSettings = appSettings;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<BaseResult> TrainModelAsync(int currencyR030)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"{_appSettings.ModelUrl}/train",
            JsonContent.Create(new { currency_r030 = currencyR030 }));

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            //TODO: Generate & send new csv file to the model and then retry
            await TrainModelAsync(currencyR030);
        }

        else if (response.StatusCode == HttpStatusCode.OK)
        {
            return TrainResult.SuccessResult(response.Content.ReadAsStringAsync().Result);
        }

        return BaseResult.FailureResult(["Failed to train the model."]);
    }

    public async Task<BaseResult> PredictAsync(int currencyR030, string date)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(
            $"{_appSettings.ModelUrl}/predict",
            JsonContent.Create(new { currency_r030 = currencyR030, date }));

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            //TODO: Call train model endpoint and then retry
        }

        else if (response.StatusCode == HttpStatusCode.OK)
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