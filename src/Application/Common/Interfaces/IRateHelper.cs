using Domain.Entities;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces;

public interface IRateHelper
{
    Task<IEnumerable<Rate>> GetRatesFromRequestAsync(ExchangeRateRequest request);
    IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates);
}