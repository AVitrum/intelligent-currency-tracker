using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IRateRepository : IBaseRepository<Rate>
{
    Task AddRangeAsync(ICollection<Rate> rates);
    Task<Rate> GetLastByCurrencyIdAsync(Guid currencyId);
    Task<IEnumerable<Rate>> GetRangeAsync(DateTime date);
    Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end, Currency currency);
    Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end, int page, int pageSize);
    Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end, Currency currency, int page, int pageSize);
    Task<DateTime> GetLastDateAsync();
}