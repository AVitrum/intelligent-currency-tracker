using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Infrastructure.Identity;
using Infrastructure.Identity.Jwt;
using Infrastructure.Identity.Results;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Shared.Payload.Requests;

namespace Infrastructure.Utils;

public class UserHelper : IUserHelper
{
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserHelper(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    public async Task ValidatePasswordAsync(ApplicationUser user, string password)
    {
        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            throw new PasswordException();
        }
    }

    public async Task CheckIfUserIsAdmin(ApplicationUser user)
    {
        IEnumerable<string> roles = await GetRolesAsync(user);

        if (!roles.Contains("ADMIN"))
        {
            throw new UnauthorizedAccessException("User is not authorized to use DevUI");
        }
    }

    public async Task<UserServiceResult> GenerateTokenResultAsync(ApplicationUser user)
    {
        IEnumerable<string> roles = await GetRolesAsync(user);
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        JwtSecurityToken token = _jwtService.GenerateToken(claims);
        RefreshToken refreshToken = await _jwtService.GenerateRefreshToken(user.Id);

        return UserServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token), refreshToken.Token);
    }

    public async Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}