using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Common.Payload.Requests;

public class LoginRequest : IUserRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public required string Password { get; set; }

    public UserServiceType ServiceType { get; set; } = UserServiceType.EMAIL;
}