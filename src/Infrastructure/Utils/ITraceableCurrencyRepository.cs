using Application.Common.Interfaces.Repositories;
using Infrastructure.Identity.SubUserEntities;

namespace Infrastructure.Utils;

public interface ITraceableCurrencyRepository : IBaseRepository<TraceableCurrency>
{
    Task<IEnumerable<TraceableCurrency>> GetByUserIdAsync(string userId);
}