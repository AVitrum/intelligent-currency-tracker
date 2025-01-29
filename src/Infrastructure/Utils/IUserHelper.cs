using Infrastructure.Identity;
using Infrastructure.Identity.Results;
using Shared.Payload;

namespace Infrastructure.Utils;

public interface IUserHelper
{
    UserLookupDelegate GetUserLookupDelegate(LoginRequest request);
    Task ValidatePasswordAsync(ApplicationUser user, string password);
    Task<UserServiceResult> GenerateTokenResultAsync(ApplicationUser user);
    Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user);
}