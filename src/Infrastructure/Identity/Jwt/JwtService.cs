using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Exceptions;
using Application.Common.Interfaces.Utils;
using Infrastructure.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Identity.Jwt;

public class JwtService : IJwtService
{
    private readonly IAppSettings _appSettings;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private string _audience = null!;
    private string _issuer = null!;
    private string _key = null!;

    public JwtService(IAppSettings appSettings, IRefreshTokenRepository refreshTokenRepository)
    {
        _appSettings = appSettings;
        _refreshTokenRepository = refreshTokenRepository;
        GetJwtConfiguration();
    }

    public JwtSecurityToken GenerateToken(ICollection<Claim> claims)
    {
        SymmetricSecurityKey generatedKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        SigningCredentials credentials = new SigningCredentials(generatedKey, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            _issuer,
            _audience,
            claims,
            expires: DateTime.UtcNow.AddDays(2),
            signingCredentials: credentials) ?? throw new JwtException("Token generation failed");

        return token;
    }

    public async Task<RefreshToken> GenerateRefreshToken(string userId)
    {
        RefreshToken? lastToken = await _refreshTokenRepository.GetByUserIdAsync(userId);

        if (lastToken is not null)
        {
            await _refreshTokenRepository.DeleteAsync(lastToken);
        }

        RefreshToken refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddMinutes(30)
        };
        await _refreshTokenRepository.AddAsync(refreshToken);

        return refreshToken;
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private void GetJwtConfiguration()
    {
        _issuer = _appSettings.JwtIssuer;
        _audience = _appSettings.JwtAudience;
        _key = _appSettings.JwtKey;
    }
}