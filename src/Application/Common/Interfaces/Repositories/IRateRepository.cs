using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IRateRepository : IBaseRepository<Rate>
{
    Task AddRangeAsync(ICollection<Rate> rates);
    Task<IEnumerable<Rate>> GetAsync(DateTime start, DateTime end);
    Task<IEnumerable<Rate>> GetAsync(DateTime start, DateTime end, Currency currency);
    Task<DateTime> GetLastDateAsync();
}