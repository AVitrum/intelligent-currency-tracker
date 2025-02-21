using System.Text.Json.Serialization;
using Domain.Enums;

namespace Shared.Payload.Requests;

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
    public string Provider { get; init; } = null!;

    [JsonIgnore] public LoginManagerProvider LoginProvider => Enum.Parse<LoginManagerProvider>(Provider, true);
}