using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Utils;
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
        ApplicationUser newUser = new()
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber ?? string.Empty,
            CreationMethod = UserCreationMethod.EMAIL
        };

        IdentityResult creationResult = await _userManager.CreateAsync(newUser);

        if (!creationResult.Succeeded)
        {
            return BaseResult.FailureResult(creationResult.Errors.Select(e => e.Description).ToList());
        }

        IdentityResult passwordResult = await _userManager.AddPasswordAsync(newUser, dto.Password);

        if (!passwordResult.Succeeded)
        {
            return BaseResult.FailureResult(passwordResult.Errors.Select(e => e.Description).ToList());
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(newUser, UserRole.USER.ToString());
        return roleResult.Succeeded
            ? BaseResult.SuccessResult()
            : BaseResult.FailureResult(roleResult.Errors.Select(e => e.Description).ToList());
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        ILoginManager manager = _loginManagerFactory.Create(request.LoginProvider);
        return await manager.LoginAsync(request);
    }

    public async Task<BaseResult> LoginWithRefreshTokenAsync(RefreshTokenRequest request)
    {
        ILoginManager manager = _loginManagerFactory.Create(request.LoginProvider);
        return await manager.LoginWithRefreshTokenAsync(request);
    }
}