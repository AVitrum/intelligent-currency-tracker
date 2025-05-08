using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class RegistrationResponse : BaseResponse
{
    public RegistrationResponse(bool success, string message, int statusCode, IEnumerable<string> errors) : base(
        success, message, statusCode, errors) { }
}