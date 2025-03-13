using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Results;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Infrastructure.Identity;

public class AdminService : IAdminService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BaseResult> CreateAsync(CreateUserDto dto)
    {
        ApplicationUser newUser = new()
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

        IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.ADMIN.ToString());

        return roleResult.Succeeded
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(roleResult.Errors.Select(error => error.Description).ToList());
    }

    public async Task<BaseResult> ChangeRoleAsync(ChangeRoleRequest request)
    {
        ApplicationUser user = await _userManager.FindByEmailAsync(request.Email)
                               ?? throw new UserNotFoundException("User not found");

        if (_httpContextAccessor.HttpContext == null)
        {
            return BaseResult.FailureResult(["Can't get user from context"]);
        }

        ApplicationUser? currentUser =
            await _userManager.FindByIdAsync(_httpContextAccessor.HttpContext.User.GetUserId()!);

        if (currentUser == user)
        {
            return BaseResult.FailureResult(["You cannot change your own role"]);
        }

        if (await _userManager.IsInRoleAsync(user, request.Role.ToString()))
        {
            return BaseResult.FailureResult([$"User is already an {request.Role}"]);
        }

        return await AddUserToRoleAsync(user, request.Role);
    }

    public async Task<BaseResult> GetAllAsync(int page, int pageSize)
    {
        List<ApplicationUser> users =
            await _userManager.Users.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        if (users.Count == 0)
        {
            return BaseResult.FailureResult(["No more users found"]);
        }

        return GetAllUsersResult.SuccessResult(users.Select(user => new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? throw new UserNotFoundException("User not found"),
            Email = user.Email ?? throw new UserNotFoundException("User not found"),
            PhoneNumber = user.PhoneNumber,
            Roles = _userManager.GetRolesAsync(user).Result,
            CreationMethod = user.CreationMethod.ToString()
        }));
    }

    public async Task<BaseResult> SearchEmailsAsync(string query)
    {
        List<string> emails = await _userManager.Users
            .Where(user => user.Email != null && user.Email.Contains(query))
            .Select(user => user.Email!)
            .ToListAsync();

        return emails.Count == 0
            ? BaseResult.FailureResult(["No users found"])
            : SearchEmailsResult.SuccessResult(emails);
    }

    public async Task<BaseResult> GetByIdAsync(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return BaseResult.FailureResult(["User not found"]);
        }

        return GetUserResult.SuccessResult(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? throw new UserNotFoundException("User not found"),
            Email = user.Email ?? throw new UserNotFoundException("User not found"),
            PhoneNumber = user.PhoneNumber,
            Roles = _userManager.GetRolesAsync(user).Result,
            CreationMethod = user.CreationMethod.ToString()
        });
    }

    private async Task<BaseResult> AddUserToRoleAsync(ApplicationUser user, UserRole role)
    {
        List<string> errors = [];
        IList<string> currentRoles = await _userManager.GetRolesAsync(user);

        foreach (string userRole in currentRoles)
        {
            IdentityResult removeResult = await _userManager.RemoveFromRoleAsync(user, userRole);

            if (!removeResult.Succeeded)
            {
                errors.AddRange(removeResult.Errors.Select(e => e.Description));
                return BaseResult.FailureResult(errors);
            }
        }

        IdentityResult addRoleResult = await _userManager.AddToRoleAsync(user, role.ToString());

        if (!addRoleResult.Succeeded)
        {
            errors.AddRange(addRoleResult.Errors.Select(e => e.Description));
            return BaseResult.FailureResult(errors);
        }

        return BaseResult.SuccessResult();
    }
}