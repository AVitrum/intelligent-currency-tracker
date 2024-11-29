using Domain.Common;

namespace Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<BaseResult> CreateUserAsync(string userName, string password);
        Task<bool> AuthorizeAsync(string userId, string policyName);
        Task<BaseResult> LoginAsync(string userName, string password);
    }
}