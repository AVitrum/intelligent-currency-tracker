using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Jwt;

public class JwtService : IJwtService
{
    private readonly IAppSettings _appSettings;
    private string _issuer = null!;
    private string _audience = null!;
    private string _key = null!;
    
    public JwtService(IAppSettings appSettings)
    {
        _appSettings = appSettings;
        GetJwtConfiguration();
    }

    private void GetJwtConfiguration()
    {
        _issuer = _appSettings.JwtIssuer;
        _audience = _appSettings.JwtAudience;
        _key = _appSettings.JwtKey;
    }

    public JwtSecurityToken GenerateToken(List<Claim> claims)
    {
        var generatedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(generatedKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials) ?? throw new JwtException("Token generation failed");
        
        return token;
    }
}