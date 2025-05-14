using Domain.Common;

namespace Shared.Payload.Responses.UserRate;

public class RemoveTrackedCurrencyResponse : BaseResponse
{
    public RemoveTrackedCurrencyResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors) : base(
        success, message, statusCode, errors) { }
}