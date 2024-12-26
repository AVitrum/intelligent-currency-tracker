using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Domain.Common;
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

    public async Task<BaseResult> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser { UserName = userName };
        IdentityResult result = await _userManager.CreateAsync(user, password);
            
        if (result.Succeeded) return BaseResult.SuccessResult();

        var errors = result.Errors.Select(error => error.Description).ToList();
        return BaseResult.FailureResult(errors);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
            
        if (user == null) return false;

        ClaimsPrincipal principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        AuthorizationResult result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<BaseResult> LoginAsync(string userName, string password)
    {
        ApplicationUser? user = await _userManager.FindByNameAsync(userName);
            
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            return BaseResult.FailureResult(["Invalid username or password."]);

        _jwtService.GetJwtConfiguration(out var issuer, out var audience, out var key);
            
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        JwtSecurityToken token = _jwtService.GenerateToken(issuer, audience, key, claims);
        
        return IdentityServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}