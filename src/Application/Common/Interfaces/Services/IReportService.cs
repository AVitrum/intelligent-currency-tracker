using Domain.Common;
using Shared.Dtos;

namespace Application.Common.Interfaces.Services;

public interface IReportService
{
    Task<BaseResult> CreateAsync(CreateReportDto dto);
    Task<BaseResult> RespondAsync(Guid id, string message);
    Task<BaseResult> GetByIdAsync(Guid id);
    Task<BaseResult> GetAllAsync(int page, int pageSize);
    Task<BaseResult> MarkAsResolvedAsync(Guid id);
}