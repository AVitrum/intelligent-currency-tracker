using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Jwt;

public class JwtService : IJwtService
{
    private readonly IAppSettings _appSettings;
    private string _audience = null!;
    private string _issuer = null!;
    private string _key = null!;

    public JwtService(IAppSettings appSettings)
    {
        _appSettings = appSettings;
        GetJwtConfiguration();
    }

    public JwtSecurityToken GenerateToken(ICollection<Claim> claims)
    {
        var generatedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(generatedKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials) ?? throw new JwtException("Token generation failed");

        return token;
    }

    private void GetJwtConfiguration()
    {
        _issuer = _appSettings.JwtIssuer;
        _audience = _appSettings.JwtAudience;
        _key = _appSettings.JwtKey;
    }
}