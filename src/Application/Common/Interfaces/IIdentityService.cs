using Application.Common.Models;
using Domain.Common;

namespace Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<BaseResult> CreateUserAsync(CreateUserModel userModel);
        Task<bool> AuthorizeAsync(string userId, string policyName);
        Task<BaseResult> LoginAsync(LoginUserModel model);
    }
}