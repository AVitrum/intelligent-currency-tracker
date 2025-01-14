using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICurrencyRepository : IBaseRepository<Currency>
{
    Task<Currency?> GetByCodeAsync(string code);
}