using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Payload.Requests;

public class LoginRequest
{
    [Required(ErrorMessage = "Email or username field is required!")]
    public string Identifier { get; set; } = null!;

    [Required(ErrorMessage = "Password field is required!")]
    public string Password { get; set; } = null!;

    public string Provider { get; init; } = null!;

    [JsonIgnore]
    public LoginManagerProvider LoginProvider => Enum.Parse<LoginManagerProvider>(Provider, true);
}