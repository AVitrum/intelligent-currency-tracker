using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Utils;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        string? userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return userId;
    }
}