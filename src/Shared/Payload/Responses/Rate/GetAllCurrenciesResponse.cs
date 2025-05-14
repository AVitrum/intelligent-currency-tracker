using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Rate;

public class GetAllCurrenciesResponse : BaseResponse
{
    public GetAllCurrenciesResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        IEnumerable<CurrencyDto>? currencies) : base(success, message, statusCode, errors)
    {
        Currencies = currencies;
    }

    public IEnumerable<CurrencyDto>? Currencies { get; }
}