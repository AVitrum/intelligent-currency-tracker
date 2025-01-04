using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.ExternalApis.GoogleAuth.Results;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.ExternalApis.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IUserFactory _userFactory;

    public GoogleAuthService(UserManager<ApplicationUser> userManager, 
        IJwtService jwtService,
        IUserFactory userFactory)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _userFactory = userFactory;
    }

    public async Task<BaseResult> HandleGoogleResponse(AuthenticateResult authResult)
    {
        if (!authResult.Succeeded)
        {
            return GoogleAuthResult.FailureResult(["Authentication failed"]);
        }

        string? email = GetEmailFromClaims(authResult);
        if (email is null)
        {
            return GoogleAuthResult.FailureResult(["Email claim not found"]);
        }

        ApplicationUser user = await EnsureUserExistsAsync(email);

        List<Claim> claims = GenerateClaims(user, email);
        JwtSecurityToken token = _jwtService.GenerateToken(claims);

        return GoogleAuthResult.SuccessResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    private async Task<ApplicationUser> EnsureUserExistsAsync(string email)
    {
        ApplicationUser? user = await GetUserByEmailAsync(email);
        if (user != null)
        {
            return user;
        }
        
        BaseResult result = await _userFactory.CreateUserAsync(
            () => new ApplicationUser
            {
                Email = email,
                UserName = email,
                EmailConfirmed = true,
                NormalizedEmail = email.ToUpper(),
                NormalizedUserName = email.ToUpper(),
                CreationMethod = UserCreationMethod.GOOGLE
            }, 
            async newUser => await _userManager.AddToRoleAsync(newUser, UserRoles.User.ToString())
        );

        if (!result.Success)
        {
            throw new Exception($"Failed to create user for email: {email}");
        }

        return await GetUserByEmailAsync(email) ?? throw new EntityNotFoundException<ApplicationUser>();
    }

    
    private static List<Claim> GenerateClaims(ApplicationUser user, string email)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        return claims;
    }


    private static string? GetEmailFromClaims(AuthenticateResult authenticateResult)
    {
        return authenticateResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;
    }

    private async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }
}