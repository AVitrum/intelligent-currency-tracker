using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IReportRepository : IBaseRepository<Report>
{
    Task<IEnumerable<Report>> GetAllAsync(int page, int pageSize);
}