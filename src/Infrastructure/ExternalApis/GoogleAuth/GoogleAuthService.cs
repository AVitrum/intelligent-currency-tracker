using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Common;
using Domain.Enums;
using Infrastructure.ExternalApis.GoogleAuth.Results;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.ExternalApis.GoogleAuth;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public GoogleAuthService(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task<BaseResult> HandleGoogleResponse(AuthenticateResult authResult)
    {
        var email = authResult.Principal?.FindFirst(ClaimTypes.Email)?.Value;
        if (email is null) return GoogleAuthResult.FailureResult(["Email claim not found"]);

        var newUser = await _userManager.FindByEmailAsync(email);
        if (newUser is not null) return await LoginAsync(newUser);

        newUser = new ApplicationUser
        {
            Email = email,
            UserName = email,
            EmailConfirmed = true,
            NormalizedEmail = email.ToUpper(),
            NormalizedUserName = email.ToUpper(),
            CreationMethod = UserCreationMethod.GOOGLE
        };

        var result = await _userManager.CreateAsync(newUser);
        if (!result.Succeeded)
            return BaseResult.FailureResult(result.Errors.Select(error => error.Description).ToList());

        var addToRoleResult = await _userManager.AddToRoleAsync(newUser, UserRole.USER.ToString());
        if (!addToRoleResult.Succeeded)
            return BaseResult.FailureResult(addToRoleResult.Errors.Select(error => error.Description).ToList());
        return await LoginAsync(newUser);
    }

    private Task<BaseResult> LoginAsync(ApplicationUser user)
    {
        ICollection<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        var token = _jwtService.GenerateToken(claims);
        return Task.FromResult<BaseResult>(
            GoogleAuthResult.SuccessResult(new JwtSecurityTokenHandler().WriteToken(token)));
    }
}