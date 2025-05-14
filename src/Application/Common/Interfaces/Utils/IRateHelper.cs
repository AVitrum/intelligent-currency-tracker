using Domain.Entities;
using Shared.Dtos;

namespace Application.Common.Interfaces.Utils;

public interface IRateHelper
{
    Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, int currencyR030);
    IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates);
    IEnumerable<CurrencyDto> ConvertCurrenciesToDtoAsync(IEnumerable<Currency> rates);
}