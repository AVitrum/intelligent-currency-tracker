using System.ComponentModel.DataAnnotations;

namespace Shared.Payload;

public class LoginRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password field is required!")]
    public string Password { get; set; } = null!;
}