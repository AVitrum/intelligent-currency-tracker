namespace Infrastructure.Data.Repositories;

public class ExchangeRateRepository : BaseRepository<ExchangeRate>, IExchangeRateRepository
{
    private readonly ApplicationDbContext _context;
    
    public ExchangeRateRepository(ApplicationDbContext context) : base(context, context.ExchangeRates)
    {
        _context = context;
    }

    public async Task AddExchangeRateRangeAsync(List<ExchangeRate> exchangeRates)
    {
        await _context.ExchangeRates.AddRangeAsync(exchangeRates);
        await _context.SaveChangesAsync();
    }

    public override async Task<IEnumerable<ExchangeRate>> GetAllAsync()
    {
        return await _context.ExchangeRates.ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllByStartDateAndEndDateAsync(DateTime start, DateTime end)
    {
        return await _context.ExchangeRates
            .Where(rate => rate.Date >= start && rate.Date <= end)
            .OrderBy(rate => rate.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllByStartDateAndEndDateAndCurrencyAsync(DateTime start, DateTime end, string currency)
    {
        return await _context.ExchangeRates
            .Where(rate => rate.Date >= start && rate.Date <= end && rate.Currency == currency)
            .OrderBy(rate => rate.Date)
            .ToListAsync();
    }

    public async Task<DateTime> GetLastDateAsync()
    {
        return await _context.ExchangeRates.MaxAsync(rate => rate.Date);
    }

    public async Task<bool> ExistsByDateAsync(DateTime date)
    {
        return await _context.ExchangeRates.AnyAsync(rate => rate.Date == date);
    }
}