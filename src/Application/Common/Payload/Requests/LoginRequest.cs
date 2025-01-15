using System.ComponentModel.DataAnnotations;

namespace Application.Common.Payload.Requests;

public class LoginRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password field is required!")]
    public required string Password { get; set; }
}