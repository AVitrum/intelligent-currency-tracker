using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces;

public interface IUserService
{
    Task<BaseResult> CreateAsync(CreateUserDto dto);
    Task<BaseResult> LoginAsync(LoginRequest request);
    Task<BaseResult> ChangeRoleAsync(ChangeRoleRequest request);
    Task<BaseResult> GetAllAsync(int page, int pageSize);
}