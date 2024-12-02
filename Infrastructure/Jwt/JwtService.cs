using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Jwt;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void GetJwtConfiguration(out string issuer, out string audience, out string key)
    {
        issuer = GetConfigurationValue("JWT_ISSUER", "Jwt:Issuer");
        audience = GetConfigurationValue("JWT_AUDIENCE", "Jwt:Audience");
        key = GetConfigurationValue("JWT_KEY", "Jwt:Key");
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
            signingCredentials: credentials);
        
        return token;
    }

    private string GetConfigurationValue(string dockerKey, string defaultKey)
    {
        return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            ? _configuration[dockerKey] ?? throw new JwtException($"{dockerKey} cannot be null")
            : _configuration[defaultKey] ?? throw new JwtException($"{defaultKey} cannot be null");
    }
}