using Application.Common.Interfaces.Repositories;
using Infrastructure.Identity.SubUserEntities;

namespace Infrastructure.Interfaces;

public interface ITraceableCurrencyRepository : IBaseRepository<TraceableCurrency>
{
    Task<bool> ExistsAsync(string userId, Guid currencyId);
    Task<TraceableCurrency> GetByUserIdAndCurrencyIdAsync(string userId, Guid currencyId);
    Task<IEnumerable<TraceableCurrency>> GetByUserIdAsync(string userId);
    Task RemoveAsync(string userId, Guid currencyId);
}