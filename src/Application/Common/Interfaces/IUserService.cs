using Application.Common.Payload.Dtos;
using Application.Common.Payload.Requests;
using Domain.Common;

namespace Application.Common.Interfaces;

public interface IUserService
{
    Task<BaseResult> CreateUserAsync(CreateUserDto dto);
    Task<BaseResult> LoginAsync(LoginRequest request);
    Task<BaseResult> ProvideAdminFunctionality(ProvideAdminFunctionalityRequest request);
}