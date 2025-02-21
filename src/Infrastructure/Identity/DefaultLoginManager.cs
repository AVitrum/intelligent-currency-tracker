using Application.Common.Exceptions;
using Application.Common.Interfaces.Utils;
using Domain.Common;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Identity;
using Shared.Payload.Requests;

namespace Infrastructure.Identity;

public class DefaultLoginManager : ILoginManager
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserHelper _userHelper;
    private readonly UserManager<ApplicationUser> _userManager;

    public DefaultLoginManager(IUserHelper userHelper, IRefreshTokenRepository refreshTokenRepository,
        UserManager<ApplicationUser> userManager)
    {
        _userHelper = userHelper;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        var lookupDelegate = _userHelper.GetUserLookupDelegate(request);
        var identifier = request.UserName ?? request.Email
            ?? throw new ArgumentException("Username or Email must be provided");

        var user = await lookupDelegate(identifier)
                   ?? throw new UserNotFoundException("User not found");

        await _userHelper.ValidatePasswordAsync(user, request.Password);
        return await _userHelper.GenerateTokenResultAsync(user);
    }

    public async Task<BaseResult> LoginWithRefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (refreshToken is null) return BaseResult.FailureResult(["Invalid refresh token"]);

        if (refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            await _refreshTokenRepository.DeleteAsync(refreshToken);
            return BaseResult.FailureResult(["Invalid refresh token"]);
        }

        var user = await _userManager.FindByIdAsync(refreshToken.UserId);

        if (user is null) return BaseResult.FailureResult(["User not found"]);

        await _refreshTokenRepository.DeleteAsync(refreshToken);
        return await _userHelper.GenerateTokenResultAsync(user);
    }
}