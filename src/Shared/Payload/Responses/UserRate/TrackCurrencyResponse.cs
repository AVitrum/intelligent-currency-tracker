using Domain.Common;

namespace Shared.Payload.Responses.UserRate;

public class TrackCurrencyResponse : BaseResponse
{
    public TrackCurrencyResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(
        success, message, statusCode, errors) { }
}