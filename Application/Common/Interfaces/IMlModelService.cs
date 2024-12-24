using Application.Common.Models;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IMlModelService
{
    Task<BaseResult> TrainModelAsync(ExchangeRatesRangeDto dto);
    Task<BaseResult> PredictAsync(ExchangeRatePredictionDto dto);
}