using Application.Common.Exceptions;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Identity;
using Shared.Payload;

namespace Infrastructure.Identity;

public class AdminUserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
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

        var passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);
        if (!passwordResult.Succeeded)
            return BaseResult.FailureResult(passwordResult.Errors.Select(error => error.Description).ToList());

        var roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.ADMIN.ToString());
        return roleResult.Succeeded
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(roleResult.Errors.Select(error => error.Description).ToList());
    }

    public Task<BaseResult> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request)
    {
        var lookupDelegate = GetUserLookupDelegate(request);

        var identifier = request.UserName ?? request.Email
            ?? throw new ArgumentException("Username or Email must be provided");

        var user = await lookupDelegate(identifier) ?? throw new UserNotFoundException("User not found");

        if (await _userManager.IsInRoleAsync(user, UserRole.ADMIN.ToString()))
            return BaseResult.FailureResult(["User is already an admin"]);

        return await AddUserToRoleAsync(user, UserRole.ADMIN.ToString());
    }

    private async Task<BaseResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (roleResult.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(user, UserRole.USER.ToString());
            return BaseResult.SuccessResult();
        }

        var errors = roleResult.Errors.Select(error => error.Description).ToList();
        return BaseResult.FailureResult(errors);
    }

    private UserLookupDelegate GetUserLookupDelegate(ProvideAdminFunctionalityRequest request)
    {
        if (request.UserName is not null) return _userManager.FindByNameAsync;

        if (request.Email is not null) return _userManager.FindByEmailAsync;

        throw new ArgumentException("Username or Email must be provided");
    }
}