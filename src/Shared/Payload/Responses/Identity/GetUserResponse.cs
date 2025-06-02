using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Identity;

public class GetUserResponse : BaseResponse
{
    public UserDto User { get; set; }

    public GetUserResponse(bool success, string message, int statusCode, IEnumerable<string> errors, UserDto user) :
        base(success, message, statusCode, errors)
    {
        User = user;
    }
}