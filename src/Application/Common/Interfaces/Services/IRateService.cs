using Domain.Common;

namespace Application.Common.Interfaces.Services;

public interface IRateService
{
    Task<BaseResult> GetRatesAsync(DateTime start, DateTime end, string? currencyString, int page, int pageSize);
    Task<BaseResult> GetAllCurrenciesAsync();
    Task<BaseResult> GetAllCurrenciesAsync(DateTime start, DateTime end);
    Task<BaseResult> GetLastUpdatedCurrenciesAsync();
    Task<BaseResult> GetDetailsAsync(string currencyCode, DateTime start, DateTime end);
    Task<BaseResult> CompareCurrenciesAsync(List<string> currencyCodes, DateTime start, DateTime end);
    Task<BaseResult> DeleteRatesAsync(string date);
}