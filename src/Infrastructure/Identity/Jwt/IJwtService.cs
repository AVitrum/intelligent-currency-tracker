using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Infrastructure.Identity.Jwt;

public interface IJwtService
{
    JwtSecurityToken GenerateToken(ICollection<Claim> claims);
    Task<RefreshToken> GenerateRefreshToken(string userId);
}