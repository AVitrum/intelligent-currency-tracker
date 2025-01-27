using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Payload;

public class CreateUserDto : IValidatableObject
{
    [Required(ErrorMessage = "Email field is required!")]
    [EmailAddress(ErrorMessage = "Email is not valid")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "UserName field is required!")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "Password field is required!")]
    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; } = string.Empty;

    public string Provider { get; set; } = null!;

    [JsonIgnore] public UserServiceProvider ServiceProvider => Enum.Parse<UserServiceProvider>(Provider, true);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(PhoneNumber) && !new PhoneAttribute().IsValid(PhoneNumber))
            yield return new ValidationResult("Phone number is not valid", [nameof(PhoneNumber)]);
    }
}