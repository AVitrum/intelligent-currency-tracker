using Domain.Common;
using Shared.Dtos;

namespace Application.Rates.Results;

public class GetAllCurrenciesResult : BaseResult
{
    private GetAllCurrenciesResult(
        bool success,
        IEnumerable<string> errors,
        IEnumerable<CurrencyDto> currencies) : base(success, errors)
    {
        Currencies = currencies;
    }

    public IEnumerable<CurrencyDto> Currencies { get; set; }

    public static GetAllCurrenciesResult SuccessResult(IEnumerable<CurrencyDto> currencies)
    {
        return new GetAllCurrenciesResult(true, [], currencies);
    }
}