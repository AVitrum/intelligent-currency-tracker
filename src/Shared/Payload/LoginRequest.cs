using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Shared.Payload;

public class LoginRequest
{
    public string? UserName { get; set; }

    public string? Email { get; set; }

    [Required(ErrorMessage = "Password field is required!")]
    public string Password { get; set; } = null!;

    public string Provider { get; set; } = null!;
    public LoginManagerProvider LoginProvider => Enum.Parse<LoginManagerProvider>(Provider, true);
}