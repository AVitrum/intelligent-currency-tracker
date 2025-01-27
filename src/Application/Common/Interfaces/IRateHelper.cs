using Application.Common.Payload.Dtos;
using Domain.Entities;
using Shared.Payload;

namespace Application.Common.Interfaces;

public interface IRateHelper
{
    Task<IEnumerable<Rate>> GetRatesFromRequestAsync(ExchangeRateRequest request);
    IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates);
}