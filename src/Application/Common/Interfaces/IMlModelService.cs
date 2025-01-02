using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IMlModelService
{
    Task<BaseResult> TrainModelAsync(ExchangeRateRequest request);
    Task<BaseResult> PredictAsync(ExchangeRatePredictionDto dto);
}