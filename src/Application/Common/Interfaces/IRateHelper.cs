using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IRateHelper
{
    Task<IEnumerable<Rate>> GetRatesFromRequestAsync(ExchangeRateRequest request);
    IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates);
}