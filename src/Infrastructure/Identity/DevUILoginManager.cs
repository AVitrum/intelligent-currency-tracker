using Application.Common.Exceptions;
using Domain.Common;
using Infrastructure.Utils;
using Shared.Payload;

namespace Infrastructure.Identity;

public class DevUILoginManager : ILoginManager
{
    private readonly IUserHelper _userHelper;

    public DevUILoginManager(IUserHelper userHelper)
    {
        _userHelper = userHelper;
    }

    public async Task<BaseResult> LoginAsync(LoginRequest request)
    {
        var lookupDelegate = _userHelper.GetUserLookupDelegate(request);
        var identifier = request.UserName ?? request.Email
            ?? throw new ArgumentException("Username or Email must be provided");

        var user = await lookupDelegate(identifier)
                   ?? throw new UserNotFoundException("User not found");

        var roles = await _userHelper.GetRolesAsync(user);
        if (!roles.Contains("ADMIN")) throw new UnauthorizedAccessException("User is not authorized to use DevUI");

        await _userHelper.ValidatePasswordAsync(user, request.Password);

        return await _userHelper.GenerateTokenResultAsync(user);
    }
}