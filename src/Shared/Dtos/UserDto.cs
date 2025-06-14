namespace Shared.Dtos;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? PhoneNumber { get; set; }
    public string? CreationMethod { get; set; }
    public IEnumerable<string> Roles { get; init; } = null!;
}