using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Identity;

namespace Infrastructure.Interfaces;

public interface IJwtService
{
    JwtSecurityToken GenerateToken(ICollection<Claim> claims);
    Task<RefreshToken> GenerateRefreshToken(string userId);
}