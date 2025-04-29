using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Services;

public interface IRateService
{
    Task<BaseResult> GetAllCurrenciesAsync();
    Task<BaseResult> GetRatesAsync(DateTime start, DateTime end, string? currencyString, int page, int pageSize);
    Task<BaseResult> DeleteRatesAsync(string date);
}