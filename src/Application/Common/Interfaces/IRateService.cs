using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IRateService
{
    Task<BaseResult> GetRatesAsync(ExchangeRateRequest request);
}