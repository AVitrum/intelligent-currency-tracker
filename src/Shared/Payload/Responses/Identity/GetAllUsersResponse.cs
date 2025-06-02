using Domain.Common;
using Shared.Dtos;

namespace Shared.Payload.Responses.Identity;

public class GetAllUsersResponse(
    bool success,
    string message,
    int statusCode,
    IEnumerable<string> errors,
    IEnumerable<UserDto> users)
    : BaseResponse(success, message, statusCode, errors)
{
    public IEnumerable<UserDto> Users { get; set; } = users;
}