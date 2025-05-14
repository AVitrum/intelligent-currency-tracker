using Application.Common.Exceptions;
using Application.Common.Interfaces.Utils;
using Domain.Common;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Shared.Payload.Requests;

namespace Infrastructure.Identity;

public class DevUILoginManager : ILoginManager
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserHelper _userHelper;
    private readonly UserManager<ApplicationUser> _userManager;

    public DevUILoginManager(
        IUserHelper userHelper,
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<ApplicationUser> userManager)
    {
        _userHelper = userHelper;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        ApplicationUser user;

        if (request.Identifier.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(request.Identifier)
                   ?? throw new UserNotFoundException("Wrong email.");
        }
        else
        {
            user = await _userManager.FindByNameAsync(request.Identifier)
                   ?? throw new UserNotFoundException("Wrong username.");
        }

        await _userHelper.CheckIfUserIsAdmin(user);
        await _userHelper.ValidatePasswordAsync(user, request.Password);

        return await _userHelper.GenerateTokenResultAsync(user);
    }

    public async Task<BaseResult> LoginWithRefreshTokenAsync(RefreshTokenRequest request)
    {
        RefreshToken? refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            return BaseResult.FailureResult(["Invalid refresh token"]);
        }

        ApplicationUser? user = await _userManager.FindByIdAsync(refreshToken.UserId);

        if (user is null)
        {
            return BaseResult.FailureResult(["User not found"]);
        }

        await _userHelper.CheckIfUserIsAdmin(user);

        return await _userHelper.GenerateTokenResultAsync(user);
    }
}