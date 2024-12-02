using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Common;
using Infrastructure.Identity.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

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
        if (!authResult.Succeeded) return GoogleAuthResult.FailureResult("Authentication failed");

        var email = GetEmailFromClaims(authResult);
        if (email is null) return GoogleAuthResult.FailureResult("Email claim not found");

        var user = await GetUserByEmailAsync(email) ?? await HandleNewUserAsync(email);

        _jwtService.GetJwtConfiguration(out var issuer, out var audience, out var key);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = _jwtService.GenerateToken(issuer, audience, key, claims);
        
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