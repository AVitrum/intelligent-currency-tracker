namespace Application.Common.Models;

public class CreateUserModel
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
}