using Domain.Common;

namespace Shared.Payload.Responses.Rate;

public class DeleteRatesResponse : BaseResponse
{
    public DeleteRatesResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(success,
        message, statusCode, errors) { }
}