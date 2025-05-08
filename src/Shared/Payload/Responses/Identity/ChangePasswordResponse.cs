using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class ChangePasswordResponse : BaseResponse
{
    public ChangePasswordResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(
        success, message, statusCode, errors) { }
}