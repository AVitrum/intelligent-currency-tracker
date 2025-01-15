using System.Globalization;
using Domain.Constans;

namespace Infrastructure.Data.Repositories;

public class RateRepository : BaseRepository<Rate>, IRateRepository
{
    private readonly ApplicationDbContext _context;

    public RateRepository(ApplicationDbContext context) : base(context, context.Rates)
    {
        _context = context;
    }

    public async Task AddRangeAsync(ICollection<Rate> rates)
    {
        await _context.Rates.AddRangeAsync(rates);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Rate>> GetAsync(DateTime start, DateTime end)
    {
        return await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date >= start && x.Date <= end)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Rate>> GetAsync(DateTime start, DateTime end, Currency currency)
    {
        return await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date >= start && x.Date <= end && x.CurrencyId == currency.Id)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<DateTime> GetLastDateAsync()
    {
        try
        {
            return await _context.Rates.MaxAsync(x => x.Date);
        }
        catch (InvalidOperationException)
        {
            return DateTime.ParseExact("01.01.2014", DateConstants.DateFormat, CultureInfo.InvariantCulture);
        }
    }
}