using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Results;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtService _jwtService;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
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

    public async Task<BaseResult> CreateUserAsync(CreateUserDto dto)
    {
        var newUser = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            CreationMethod = UserCreationMethod.EMAIL
        };

        var creationResult = await _userManager.CreateAsync(newUser);
        if (!creationResult.Succeeded)
            return BaseResult.FailureResult(creationResult.Errors.Select(e => e.Description).ToList());

        var passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);
        if (!passwordResult.Succeeded)
            return BaseResult.FailureResult(passwordResult.Errors.Select(e => e.Description).ToList());

        var roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.User.ToString());
        return roleResult.Succeeded
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(roleResult.Errors.Select(e => e.Description).ToList());
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        var lookupDelegate = GetUserLookupDelegate(request);
        var identifier = request.UserName ?? request.Email
            ?? throw new ArgumentException("Username or Email must be provided");

        var user = await lookupDelegate(identifier)
                   ?? throw new UserNotFoundException("User not found");

        await ValidatePasswordAsync(user, request.Password);

        return await GenerateTokenResultAsync(user);
    }

    public Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request)
    {
        throw new NotImplementedException();
    }

    // Збережений закоментований метод
    // public async Task<bool> AuthorizeAsync(string userId, string policyName)
    // {
    //     ApplicationUser? user = await _userManager.FindByIdAsync(userId);
    //     if (user is null)
    //     {
    //         return false;
    //     }
    //
    //     ClaimsPrincipal principal = await _userClaimsPrincipalFactory.CreateAsync(user);
    //     AuthorizationResult result = await _authorizationService.AuthorizeAsync(principal, policyName);
    //
    //     return result.Succeeded;
    // }

    private UserLookupDelegate GetUserLookupDelegate(LoginRequest request)
    {
        return request.UserName is not null
            ? _userManager.FindByNameAsync
            : request.Email is not null
                ? _userManager.FindByEmailAsync
                : throw new ArgumentException("Username or Email must be provided");
    }

    private async Task ValidatePasswordAsync(ApplicationUser user, string password)
    {
        if (!await _userManager.CheckPasswordAsync(user, password)) throw new PasswordException();
    }

    private async Task<IdentityServiceResult> GenerateTokenResultAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = _jwtService.GenerateToken(claims);

        return IdentityServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}