using Application.Common.Exceptions;
using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Utils;
using Microsoft.AspNetCore.Identity;

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

        IdentityResult passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);
        if (!passwordResult.Succeeded)
        {
            return BaseResult.FailureResult(passwordResult.Errors.Select(error => error.Description).ToList());
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.Admin.ToString());
        if (!roleResult.Succeeded)
        {
            return BaseResult.FailureResult(roleResult.Errors.Select(error => error.Description).ToList());
        }

        return BaseResult.SuccessResult();
    }

    public Task<BaseResult> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request)
    {
        UserLookupDelegate lookupDelegate = GetUserLookupDelegate(request);

        string identifier = request.UserName ?? request.Email 
            ?? throw new ArgumentException("Username or Email must be provided");
        
        ApplicationUser user = await lookupDelegate(identifier) ?? throw new UserNotFoundException("User not found");
        
        if (await _userManager.IsInRoleAsync(user, UserRole.Admin.ToString()))
        {
            return BaseResult.FailureResult(["User is already an admin"]);
        }
        
        return await AddUserToRoleAsync(user, UserRole.Admin.ToString());
    }

    private async Task<BaseResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role);
        if (roleResult.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(user, UserRole.User.ToString());
            return BaseResult.SuccessResult();
        }
        var errors = roleResult.Errors.Select(error => error.Description).ToList();
        return BaseResult.FailureResult(errors);
    }
    
    private UserLookupDelegate GetUserLookupDelegate(ProvideAdminFunctionalityRequest request)
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
}