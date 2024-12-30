using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Common;
using Infrastructure.ExternalApis.GoogleAuth.Results;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalApis.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public GoogleAuthService(
        ILogger<GoogleAuthService> logger,
        UserManager<ApplicationUser> userManager, 
        IJwtService jwtService)
    {
        _logger = logger;
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<BaseResult> HandleGoogleResponse(AuthenticateResult authResult)
    {
        if (!authResult.Succeeded)
        {
            return GoogleAuthResult.FailureResult("Authentication failed");
        }

        string? email = GetEmailFromClaims(authResult);
        if (email is null)
        {
            return GoogleAuthResult.FailureResult("Email claim not found");
        }

        ApplicationUser user = await GetUserByEmailAsync(email) ?? await HandleNewUserAsync(email);

        _jwtService.GetJwtConfiguration(out string issuer, out string audience, out string key);
        
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        
        JwtSecurityToken token = _jwtService.GenerateToken(issuer, audience, key, claims);
        
        return GoogleAuthResult.SuccessResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
    
    private static string? GetEmailFromClaims(AuthenticateResult authenticateResult)
    {
        return authenticateResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;
    }

    private async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    private async Task<ApplicationUser> HandleNewUserAsync(string email)
    {
        _logger.LogInformation("User not found, creating new user");
        var newUser = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
        };
        await _userManager.CreateAsync(newUser);
        
        return (await GetUserByEmailAsync(email))!;
    }
}