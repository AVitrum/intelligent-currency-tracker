using Application.Common.Exceptions;
using Application.Common.Interfaces.Utils;
using Domain.Common;
using Infrastructure.Utils;
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
        UserLookupDelegate lookupDelegate = _userHelper.GetUserLookupDelegate(request);
        string identifier = request.UserName ?? request.Email
            ?? throw new ArgumentException("Username or Email must be provided");

        ApplicationUser user = await lookupDelegate(identifier)
                               ?? throw new UserNotFoundException("User not found");

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