using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IExchangeRateRepository : IBaseRepository<ExchangeRate>
{
    Task SaveExchangeRatesAsync(List<ExchangeRate> exchangeRates);
    Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(DateTime start, DateTime end, Currency currency);
}