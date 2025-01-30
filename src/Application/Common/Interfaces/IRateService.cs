using Domain.Common;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces;

public interface IRateService
{
    Task<BaseResult> GetRatesAsync(ExchangeRateRequest request);
}