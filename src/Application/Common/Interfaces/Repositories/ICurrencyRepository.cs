using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface ICurrencyRepository : IBaseRepository<Currency>
{
    Task<Currency?> GetByCodeAsync(string code);
    Task<Currency?> GetByR030Async(int r030);
}