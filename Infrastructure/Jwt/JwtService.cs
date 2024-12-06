using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Jwt;

public class JwtService : IJwtService
{
    private readonly IAppSettings _appSettings;

    public JwtService(IAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    public void GetJwtConfiguration(out string issuer, out string audience, out string key)
    {
        issuer = _appSettings.JwtIssuer; 
        audience = _appSettings.JwtAudience;
        key = _appSettings.JwtKey;
    }

    public JwtSecurityToken GenerateToken(string issuer, string audience, string key, Claim[] claims)
    {
        var generatedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(generatedKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials) ?? throw new JwtException("Token generation failed");
        
        return token;
    }
}