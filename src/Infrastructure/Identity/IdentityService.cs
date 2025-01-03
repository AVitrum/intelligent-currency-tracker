using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Payload.Requests;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Results;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtService _jwtService;
    private readonly UserFactory _userFactory;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IJwtService jwtService,
        UserFactory userFactory)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _jwtService = jwtService;
        _userFactory = userFactory;
    }

    //TODO: Add phone & email validation
    public async Task<BaseResult> CreateUserAsync(CreateUserRequest request)
    {
        return await _userFactory.CreateUserAsync(
            () => new ApplicationUser 
            {
                UserName = request.UserName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber ?? string.Empty 
            },
            async user =>
            {
                IdentityResult passwordResult = await _userManager.AddPasswordAsync(user, request.Password);
                if (!passwordResult.Succeeded)
                {
                    return passwordResult;
                }

                return await _userManager.AddToRoleAsync(user, UserRoles.User.ToString());
            });
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        ClaimsPrincipal principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        AuthorizationResult result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        UserLookupDelegate lookupDelegate = GetUserLookupDelegate(request);

        string identifier = request.UserName ?? request.Email 
            ?? throw new ArgumentException("Username or Email must be provided");

        ApplicationUser user = await lookupDelegate(identifier) 
                               ?? throw new UserNotFoundException("User not found");

        await ValidatePasswordAsync(user, request.Password);

        return await GenerateTokenResultAsync(user);
    }
    
    private UserLookupDelegate GetUserLookupDelegate(LoginRequest request)
    {
        if (request.UserName is not null)
        {
            return _userManager.FindByNameAsync;
        }

        if (request.Email is not null)
        {
            return _userManager.FindByEmailAsync;
        }

        throw new ArgumentException("Username or Email must be provided");
    }
    
    private async Task ValidatePasswordAsync(ApplicationUser user, string password)
    {
        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            throw new PasswordException();
        }
    }
    
    private async Task<BaseResult> GenerateTokenResultAsync(ApplicationUser user)
    {
        _jwtService.GetJwtConfiguration(out string issuer, out string audience, out string key);

        IList<string> roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        claims.AddRange(roleClaims);

        JwtSecurityToken token = _jwtService.GenerateToken(issuer, audience, key, claims);

        return IdentityServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}