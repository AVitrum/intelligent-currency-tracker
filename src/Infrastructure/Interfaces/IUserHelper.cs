using Infrastructure.Identity;
using Infrastructure.Identity.Results;

namespace Infrastructure.Interfaces;

public interface IUserHelper
{
    Task ValidatePasswordAsync(ApplicationUser user, string password);
    Task CheckIfUserIsAdmin(ApplicationUser user);
    Task<UserServiceResult> GenerateTokenResultAsync(ApplicationUser user);
    Task<IEnumerable<string>> GetRolesAsync(ApplicationUser user);
}