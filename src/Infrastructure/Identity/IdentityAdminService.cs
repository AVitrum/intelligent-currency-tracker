using Application.Common.Exceptions;
using Application.Common.Payload.Requests;
using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public class IdentityAdminService : IIdentityAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityAdminService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request)
    {
        ApplicationUser user = await FindUserAsync(request.Username, request.Email);
        if (await _userManager.IsInRoleAsync(user, UserRoles.Admin.ToString()))
        {
            return BaseResult.FailureResult(["User is already an admin"]);
        }
        
        return await AddUserToRoleAsync(user, UserRoles.Admin.ToString());
    }
    
    private async Task<ApplicationUser> FindUserAsync(string? username, string? email)
    {
        if (username is not null)
        {
            return await _userManager.FindByNameAsync(username) ??
                   throw new UserNotFoundException("User not found by username");
        }
        if (email is not null)
        {
            return await _userManager.FindByEmailAsync(email) ??
                   throw new UserNotFoundException("User not found by email");
        }
        throw new UserNotFoundException("Email or username must be provided");
    }

    private async Task<BaseResult> AddUserToRoleAsync(ApplicationUser user, string role)
    {
        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role);
        if (roleResult.Succeeded)
        {
            await _userManager.RemoveFromRoleAsync(user, UserRoles.User.ToString());
            return BaseResult.SuccessResult();
        }
        var errors = roleResult.Errors.Select(error => error.Description).ToList();
        return BaseResult.FailureResult(errors);
    }
}