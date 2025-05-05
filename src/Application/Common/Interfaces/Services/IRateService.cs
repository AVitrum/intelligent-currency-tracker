using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface IRateService
{
    Task<BaseResult> GetRatesAsync(DateTime start, DateTime end, string? currencyString, int page, int pageSize);
    Task<BaseResult> GetAllCurrenciesAsync();
    Task<BaseResult> GetLastUpdatedCurrenciesAsync();
    Task<BaseResult> DeleteRatesAsync(string date);
}