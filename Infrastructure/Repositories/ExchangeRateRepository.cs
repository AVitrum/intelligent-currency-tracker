using Domain.Enums;

namespace Infrastructure.Repositories;

public class ExchangeRateRepository : BaseRepository<ExchangeRate>, IExchangeRateRepository
{
    private readonly ApplicationDbContext _context;
    
    public ExchangeRateRepository(ApplicationDbContext context) : base(context, context.ExchangeRates)
    {
        _context = context;
    }

    public async Task SaveExchangeRatesAsync(List<ExchangeRate> exchangeRates)
    {
        await _context.ExchangeRates.AddRangeAsync(exchangeRates);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(DateTime start, DateTime end, Currency currency)
    {
        if (currency.Equals(Currency.ALL))
            return await _context.ExchangeRates
                .Where(rate => rate.Date >= start && rate.Date <= end)
                .OrderBy(rate => rate.Date)
                .ToListAsync();
        
        return await _context.ExchangeRates
            .Where(rate => rate.Date >= start && rate.Date <= end && rate.Currency == currency)
            .OrderBy(rate => rate.Date)
            .ToListAsync();
    }
}