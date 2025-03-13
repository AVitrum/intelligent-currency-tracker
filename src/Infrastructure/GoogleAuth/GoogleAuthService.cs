using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces.Services;
using Domain.Common;
using Domain.Enums;
using Infrastructure.GoogleAuth.Results;
using Infrastructure.Identity;
using Infrastructure.Identity.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GoogleAuthService(
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager)
    {
        _jwtService = jwtService;
        _userManager = userManager;
    }

    public async Task<BaseResult> HandleGoogleResponse(AuthenticateResult authResult)
    {
        string? email = authResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;

        if (email is null)
        {
            return GoogleAuthResult.FailureResult(["Email claim not found"]);
        }

        ApplicationUser? newUser = await _userManager.FindByEmailAsync(email);

        if (newUser is not null)
        {
            return await LoginAsync(newUser);
        }

        newUser = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
            CreationMethod = UserCreationMethod.GOOGLE
        };

        IdentityResult result = await _userManager.CreateAsync(newUser);

        if (!result.Succeeded)
        {
            return BaseResult.FailureResult(result.Errors.Select(error => error.Description).ToList());
        }

        IdentityResult addToRoleResult = await _userManager.AddToRoleAsync(newUser, UserRole.USER.ToString());

        if (!addToRoleResult.Succeeded)
        {
            return BaseResult.FailureResult(addToRoleResult.Errors.Select(error => error.Description).ToList());
        }

        return await LoginAsync(newUser);
    }

    private async Task<BaseResult> LoginAsync(ApplicationUser user)
    {
        ICollection<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        JwtSecurityToken token = _jwtService.GenerateToken(claims);
        RefreshToken refreshToken = await _jwtService.GenerateRefreshToken(user.Id);

        return GoogleAuthResult.SuccessResult(new JwtSecurityTokenHandler().WriteToken(token), refreshToken.Token);
    }
}