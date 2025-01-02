namespace Application.Common.Payload.Requests;

public class CreateUserRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
}