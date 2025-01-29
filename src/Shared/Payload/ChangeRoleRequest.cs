using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Payload;

public class ChangeRoleRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }

    public string RoleString { get; set; } = "User";

    [JsonIgnore] public UserRole Role => Enum.Parse<UserRole>(RoleString, true);
}