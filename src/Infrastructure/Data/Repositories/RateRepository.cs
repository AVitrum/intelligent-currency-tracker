using System.Globalization;
using Application.Common.Interfaces.Repositories;
using Domain.Constants;

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

    public async Task<IEnumerable<Rate>> GetAsync(DateTime date)
    {
        return await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date == date.Date)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Rate>> GetAsync(
        DateTime start,
        DateTime end,
        Currency currency,
        int page,
        int pageSize)
    {
        var rates = await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date >= start.Date && x.Date.Date <= end.Date && x.CurrencyId == currency.Id)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return rates;
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