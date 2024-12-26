using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Common.Interfaces;

public interface IJwtService
{
    void GetJwtConfiguration(out string issuer, out string audience, out string key);
    JwtSecurityToken GenerateToken(string issuer, string audience, string key, Claim[] claims);
}