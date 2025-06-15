using Domain.Common;

namespace Shared.Payload.Responses.Identity;

public class UserRoleResponse : BaseResponse
{
    public string Role { get; private set; }

    public UserRoleResponse(bool success, string message, int statusCode, IEnumerable<string> errors, string role) :
        base(success, message, statusCode, errors)
    {
        Role = role;
    }
}