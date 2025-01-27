using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Payload;

public class ProvideAdminFunctionalityRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }

    [Required(ErrorMessage = "RoleString field is required!")]
    public required string RoleString { get; set; }

    [JsonIgnore] public UserRole Role => Enum.Parse<UserRole>(RoleString);
}