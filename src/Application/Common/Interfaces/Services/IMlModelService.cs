using Application.Common.Payload.Dtos;
using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Services;

public interface IMlModelService
{
    Task<BaseResult> TrainModelAsync(ExchangeRateRequest request);
    Task<BaseResult> PredictAsync(ExchangeRatePredictionDto dto);
}