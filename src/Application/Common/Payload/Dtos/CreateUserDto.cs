using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Application.Common.Payload.Dtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "Email field is required!")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "UserName field is required!")]
    public required string UserName { get; set; }

    [Required(ErrorMessage = "Password field is required!")]
    public required string Password { get; set; }

    [Phone(ErrorMessage = "Phone number is not valid")]
    public string? PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Provider field is required!")]
    public required string Provider { get; set; }

    [JsonIgnore] public UserServiceProvider ServiceProvider => Enum.Parse<UserServiceProvider>(Provider, true);
}