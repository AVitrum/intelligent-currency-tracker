using Application.Common.Interfaces;
using Domain.Enums;

namespace Application.Common.Payload.Requests;

public class CreateUserRequest : IUserRequest
{
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
    
    public UserServiceType ServiceType { get; set; } = UserServiceType.EMAIL;
}