using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Infrastructure.Identity;
using Infrastructure.Identity.Jwt;
using Infrastructure.Identity.Results;
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

    public UserLookupDelegate GetUserLookupDelegate(LoginRequest request)
    {
        return request.UserName is not null
            ? _userManager.FindByNameAsync
            : request.Email is not null
                ? _userManager.FindByEmailAsync
                : throw new ArgumentException("Username or Email must be provided");
    }

    public async Task ValidatePasswordAsync(ApplicationUser user, string password)
    {
        if (!await _userManager.CheckPasswordAsync(user, password)) throw new PasswordException();
    }

    public async Task CheckIfUserIsAdmin(ApplicationUser user)
    {
        var roles = await GetRolesAsync(user);

        if (!roles.Contains("ADMIN")) throw new UnauthorizedAccessException("User is not authorized to use DevUI");
    }

    public async Task<UserServiceResult> GenerateTokenResultAsync(ApplicationUser user)
    {
        var roles = await GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = _jwtService.GenerateToken(claims);
        var refreshToken = await _jwtService.GenerateRefreshToken(user.Id);

        return UserServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token), refreshToken.Token);
    }

    public async Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}