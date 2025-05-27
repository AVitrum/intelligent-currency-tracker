using Domain.Entities;
using Shared.Dtos;

namespace Application.Common.Interfaces.Utils;

public interface IRateHelper
{
    Task<IEnumerable<Rate>> GetRatesAsync(DateTime start, DateTime end, int currencyR030);
    IEnumerable<RateDto> ConvertRatesToDtoAsync(IEnumerable<Rate> rates);
    IEnumerable<CurrencyDto> ConvertCurrenciesToDtoAsync(IEnumerable<Currency> rates);
    SingleCurrencyAnalyticsDto AnalyzeCurrency(List<Rate> rates, string currencyCode);

    ComparativeAnalyticsDto CompareCurrencies(
        List<SingleCurrencyAnalyticsDto> analysis,
        List<List<Rate>> ratesLists,
        DateTime start,
        DateTime end);
}