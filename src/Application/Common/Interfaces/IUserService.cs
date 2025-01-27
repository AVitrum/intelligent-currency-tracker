using Domain.Common;
using Shared.Payload;

namespace Application.Common.Interfaces;

public interface IUserService
{
    Task<BaseResult> CreateUserAsync(CreateUserDto dto);
    Task<BaseResult> LoginAsync(LoginRequest request);
    Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request);
}