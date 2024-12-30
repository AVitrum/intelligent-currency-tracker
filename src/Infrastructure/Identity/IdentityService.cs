using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Models;
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

    //TODO: Add phone number validation
    public async Task<BaseResult> CreateUserAsync(CreateUserModel userModel)
    {
        var user = new ApplicationUser
        {
            UserName = userModel.UserName,
            Email = userModel.Email,
            PhoneNumber = userModel.PhoneNumber
        };
        
        IdentityResult result = await _userManager.CreateAsync(user, userModel.Password);
        if (result.Succeeded)
        {
            return BaseResult.SuccessResult();
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

    public async Task<BaseResult> LoginAsync(LoginUserModel model)
    {
        if (model.UserName is not null)
        {
            ApplicationUser? user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new UserNotFoundException("User not found or password is incorrect");
            }

            _jwtService.GetJwtConfiguration(out string issuer, out string audience, out string key);
            
            Claim[] claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];
            JwtSecurityToken token = _jwtService.GenerateToken(issuer, audience, key, claims);
        
            return IdentityServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token));
        }
        
        if (model.Email is not null)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new UserNotFoundException("User not found or password is incorrect");
            }

            _jwtService.GetJwtConfiguration(out string issuer, out string audience, out string key);
            
            Claim[] claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ];
            JwtSecurityToken token = _jwtService.GenerateToken(issuer, audience, key, claims);
        
            return IdentityServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        throw new UserNotFoundException("User not found or password is incorrect");
    }
}