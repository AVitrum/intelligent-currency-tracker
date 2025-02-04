using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Infrastructure.Identity;

public class UserService : IUserService
{
    private readonly ILoginManagerFactory _loginManagerFactory;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        ILoginManagerFactory loginManagerFactory)
    {
        _userManager = userManager;
        _loginManagerFactory = loginManagerFactory;
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

        var creationResult = await _userManager.CreateAsync(newUser);
        if (!creationResult.Succeeded)
            return BaseResult.FailureResult(creationResult.Errors.Select(e => e.Description).ToList());

        var passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);
        if (!passwordResult.Succeeded)
            return BaseResult.FailureResult(passwordResult.Errors.Select(e => e.Description).ToList());

        var roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.USER.ToString());
        return roleResult.Succeeded
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(roleResult.Errors.Select(e => e.Description).ToList());
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        var manager = _loginManagerFactory.Create(request.LoginProvider);
        return await manager.LoginAsync(request);
    }

    public Task<BaseResult> ChangeRoleAsync(ChangeRoleRequest request)
    {
        return Task.FromResult(BaseResult.FailureResult(["You don't have permission to change roles"]));
    }

    public Task<BaseResult> GetAllAsync(int page, int pageSize)
    {
        return Task.FromResult(BaseResult.FailureResult(["You don't have permission to perform this action"]));
    }

    public Task<BaseResult> SearchEmailsAsync(string query)
    {
        return Task.FromResult(BaseResult.FailureResult(["You don't have permission to perform this action"]));
    }
}