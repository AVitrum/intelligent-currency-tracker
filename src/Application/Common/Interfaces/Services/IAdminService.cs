using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Requests;

namespace Application.Common.Interfaces.Services;

public interface IAdminService
{
    Task<BaseResult> CreateAsync(CreateUserDto dto);
    Task<BaseResult> ChangeRoleAsync(ChangeRoleRequest request);
    Task<BaseResult> GetAllAsync(int page, int pageSize);
    Task<BaseResult> SearchEmailsAsync(string query);
    Task<BaseResult> GetByIdAsync(string id);
}