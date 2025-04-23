using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface ITraceableCurrencyService
{
    Task<BaseResult> TrackCurrencyAsync(string userId, int currencyR030);
    Task<BaseResult> GetTrackedCurrenciesAsync(string userId);
}