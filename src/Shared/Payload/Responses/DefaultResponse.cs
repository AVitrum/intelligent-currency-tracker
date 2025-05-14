using Domain.Common;

namespace Shared.Payload.Responses;

public class DefaultResponse : BaseResponse
{
    public DefaultResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(success,
        message, statusCode, errors) { }
}