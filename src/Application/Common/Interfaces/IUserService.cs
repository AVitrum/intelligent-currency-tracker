using Domain.Common;

namespace Application.Common.Interfaces;

public interface IUserService
{
    Task<BaseResult> CreateUserAsync(IUserRequest request);
    Task<BaseResult> LoginAsync(IUserRequest request);
}