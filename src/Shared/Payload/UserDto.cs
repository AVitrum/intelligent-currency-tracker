namespace Shared.Payload;

public class UserDto
{
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string CreationMethod { get; set; } = null!;
    public IEnumerable<string> Roles { get; set; } = null!;
}