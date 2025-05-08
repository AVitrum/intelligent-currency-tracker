using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface ITraceableCurrencyService
{
    Task<BaseResult> TrackCurrencyAsync(string userId, string currencyCode);
    Task<BaseResult> GetTrackedCurrenciesAsync(string userId);
    Task<BaseResult> RemoveTrackedCurrency(string userId, string currencyCode);
}