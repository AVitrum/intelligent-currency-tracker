using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.UserRate;

public class GetTrackedCurrenciesResponse : BaseResponse
{
    public GetTrackedCurrenciesResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        IEnumerable<CurrencyDto>? data) : base(success, message, statusCode, errors)
    {
        Data = data;
    }

    public IEnumerable<CurrencyDto>? Data { get; }
}