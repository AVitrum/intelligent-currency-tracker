using Domain.Common;
using Shared.Dtos;

namespace Application.Rates.Results;

public class GetAllCurrenciesResult : BaseResult
{
    public IEnumerable<CurrencyDto> Currencies { get; set; }
    
    private GetAllCurrenciesResult(bool success, IEnumerable<string> errors, IEnumerable<CurrencyDto> currencies) : base(success, errors)
    {
        Currencies = currencies;
    }
    
    public static GetAllCurrenciesResult SuccessResult(IEnumerable<CurrencyDto> currencies) 
        => new GetAllCurrenciesResult(true, [], currencies);
}