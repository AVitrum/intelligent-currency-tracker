using Application.Common.Payload.Dtos;
using Domain.Common;
using Shared.Payload;

namespace Application.Common.Interfaces;

public interface IMlModelService
{
    Task<BaseResult> TrainModelAsync(ExchangeRateRequest request);
    Task<BaseResult> PredictAsync(ExchangeRatePredictionDto dto);
}