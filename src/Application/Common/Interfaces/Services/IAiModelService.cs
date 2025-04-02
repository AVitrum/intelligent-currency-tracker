using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface IAiModelService
{
    Task<BaseResult> TrainModelAsync(int currencyR030);
    Task<BaseResult> PredictAsync(int currencyR030, string date);
}