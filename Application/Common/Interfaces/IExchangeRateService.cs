using Application.Common.Models;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IExchangeRateService
{
    Task<BaseResult> FetchExchangeRatesAsync(ExchangeRatesRangeDto dto);
    Task<BaseResult> GetRangeAsync(ExchangeRatesRangeDto dto);
}