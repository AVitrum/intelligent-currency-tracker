using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Exceptions;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Results;
using Infrastructure.Identity.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtService _jwtService;

    public UserService(UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService, IJwtService jwtService)
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
        await _userManager.CreateAsync(newUser);

        IdentityResult passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);
        if (!passwordResult.Succeeded)
        {
            return BaseResult.FailureResult(passwordResult.Errors.Select(error => error.Description).ToList());
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.User.ToString());
        if (!roleResult.Succeeded)
        {
            return BaseResult.FailureResult(roleResult.Errors.Select(error => error.Description).ToList());
        }

        return BaseResult.SuccessResult();
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

    public Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request)
    {
        throw new NotImplementedException();
    }

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

    private async Task<IdentityServiceResult> GenerateTokenResultAsync(ApplicationUser user)
    {
        IList<string> roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

                        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        claims.AddRange(roleClaims);

        JwtSecurityToken token = _jwtService.GenerateToken(claims);

        return IdentityServiceResult.ReturnTokenResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}