using Domain.Common;

namespace Shared.Payload.Responses;

public class DefaultErrorResponse : BaseResponse
{
    public DefaultErrorResponse(
        bool success,
        string message,
        int statusCode,
        IEnumerable<string> errors,
        string traceId)
        : base(success, message, statusCode, errors)
    {
        TraceId = traceId;
    }

    public string TraceId { get; set; }
}