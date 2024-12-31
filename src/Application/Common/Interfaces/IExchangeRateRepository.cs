using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IExchangeRateRepository : IBaseRepository<ExchangeRate>
{
    Task SaveExchangeRatesAsync(List<ExchangeRate> exchangeRates);
    Task<IEnumerable<ExchangeRate>> GetAllByStartDateAndEndDateAsync(DateTime start, DateTime end);
    Task<IEnumerable<ExchangeRate>> GetAllByStartDateAndEndDateAndCurrencyAsync(DateTime start, DateTime end, string currency);
    
    Task<bool> ExistsByDateAsync(DateTime date);
}