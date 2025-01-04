using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Common.Interfaces;

public interface IJwtService
{
    JwtSecurityToken GenerateToken(List<Claim> claims);
}