using Application.Common.Exceptions;
using Domain.Common;
using Domain.Enums;
using Infrastructure.Identity.Results;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Infrastructure.Identity;

public class AdminUserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUserService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<BaseResult> CreateAsync(CreateUserDto dto)
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

    public async Task<BaseResult> ChangeRoleAsync(ChangeRoleRequest request)
    {
        var lookupDelegate = GetUserLookupDelegate(request);

        var identifier = request.UserName ?? request.Email
            ?? throw new ArgumentException("Username or Email must be provided");

        var user = await lookupDelegate(identifier) ?? throw new UserNotFoundException("User not found");

        if (await _userManager.IsInRoleAsync(user, request.Role.ToString()))
            return BaseResult.FailureResult([$"User is already an {request.Role}"]);

        return await AddUserToRoleAsync(user, request.Role);
    }

    //TODO: Test this method
    public async Task<BaseResult> GetAllAsync(int page, int pageSize)
    {
        var users = await _userManager.Users.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        if (users.Count == 0) return BaseResult.FailureResult(["No more users found"]);

        return GetUserResult.SuccessResult(users.Select(user => new UserDto
        {
            UserName = user.UserName ?? throw new UserNotFoundException("User not found"),
            Email = user.Email ?? throw new UserNotFoundException("User not found"),
            PhoneNumber = user.PhoneNumber,
            Roles = _userManager.GetRolesAsync(user).Result
        }));
    }

    private async Task<BaseResult> AddUserToRoleAsync(ApplicationUser user, UserRole role)
    {
        var errors = new List<string>();
        var currentRoles = await _userManager.GetRolesAsync(user);

        foreach (var userRole in currentRoles)
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, userRole);

            if (!removeResult.Succeeded)
            {
                errors.AddRange(removeResult.Errors.Select(e => e.Description));
                return BaseResult.FailureResult(errors);
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role.ToString());

        if (!addRoleResult.Succeeded)
        {
            errors.AddRange(addRoleResult.Errors.Select(e => e.Description));
            return BaseResult.FailureResult(errors);
        }

        return BaseResult.SuccessResult();
    }

    private UserLookupDelegate GetUserLookupDelegate(ChangeRoleRequest request)
    {
        if (request.UserName is not null) return _userManager.FindByNameAsync;

        if (request.Email is not null) return _userManager.FindByEmailAsync;

        throw new ArgumentException("Username or Email must be provided");
    }
}