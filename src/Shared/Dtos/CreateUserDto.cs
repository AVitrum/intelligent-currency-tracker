using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Dtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email field is required!")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "UserName field is required!")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Password field is required!")]
    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; } = string.Empty;

    public string Provider { get; set; } = "default";

    [JsonIgnore] public UserServiceProvider ServiceProvider => Enum.Parse<UserServiceProvider>(Provider, true);
}