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
        UserLookupDelegate lookupDelegate = request.Username is not null 
            ? _userManager.FindByNameAsync 
            : _userManager.FindByEmailAsync;
        
        string identifier = request.Username ?? request.Email 
            ?? throw new UserNotFoundException("Email or username must be provided");
        
        ApplicationUser user = await lookupDelegate(identifier) ?? throw new UserNotFoundException("User not found");
        
        if (await _userManager.IsInRoleAsync(user, UserRoles.Admin.ToString()))
        {
            return BaseResult.FailureResult(["User is already an admin"]);
        }
        
        return await AddUserToRoleAsync(user, UserRoles.Admin.ToString());
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