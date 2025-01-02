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

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _jwtService = jwtService;
    }

    //TODO: Add phone number validation
    public async Task<BaseResult> CreateUserAsync(CreateUserRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber
        };
        
        IdentityResult result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded)
        {
            IdentityResult roleResult = await _userManager.AddToRoleAsync(user, UserRoles.User.ToString());
            if (roleResult.Succeeded)
            {
                return BaseResult.SuccessResult();
            }

            var roleErrors = roleResult.Errors.Select(error => error.Description).ToList();
            return BaseResult.FailureResult(roleErrors);
        }

        var errors = result.Errors.Select(error => error.Description).ToList();
        return BaseResult.FailureResult(errors);
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
        UserLookupDelegate lookupDelegate = request.UserName is not null 
            ? _userManager.FindByNameAsync 
            : _userManager.FindByEmailAsync;

        string identifier = request.UserName ?? request.Email 
            ?? throw new ArgumentException("Username or Email must be provided");

        ApplicationUser user = await lookupDelegate(identifier) ?? throw new UserNotFoundException("User not found");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new PasswordException();
        }

        return await GenerateTokenResultAsync(user);
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