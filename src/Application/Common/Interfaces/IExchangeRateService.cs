using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IExchangeRateService
{
    Task<BaseResult> FetchExchangeRatesAsync(ExchangeRateRequest request);
    Task<BaseResult> GetRangeAsync(ExchangeRateRequest request);
}