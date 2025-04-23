using Domain.Entities;
using Shared.Dtos;

namespace Application.Common.Interfaces.Utils;

public interface IRateHelper
{
    Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, string? currencyCode, int page, int pageSize);
    Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, int currencyR030);
    IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates);
    Task<bool> DeleteRatesAsync(DateTime date);
}