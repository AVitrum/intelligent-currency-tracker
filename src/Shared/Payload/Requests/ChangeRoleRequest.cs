using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Payload.Requests;

public class ChangeRoleRequest
{
    [Required(ErrorMessage = "Email is required!"), EmailAddress(ErrorMessage = "Wrong format!")]
    public string Email { get; set; } = null!;

    public string RoleString { get; set; } = "User";

    [JsonIgnore] public UserRole Role => Enum.Parse<UserRole>(RoleString, true);
}