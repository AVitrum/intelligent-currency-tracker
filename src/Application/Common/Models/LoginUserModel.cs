namespace Application.Common.Models;

public class LoginUserModel
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public required string Password { get; set; }
}