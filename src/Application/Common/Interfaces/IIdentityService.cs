using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IIdentityService
{
    Task<BaseResult> CreateUserAsync(CreateUserRequest request);
    Task<bool> AuthorizeAsync(string userId, string policyName);
    Task<BaseResult> LoginAsync(LoginRequest request);
}