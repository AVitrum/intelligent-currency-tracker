using System.Globalization;
using Application.Common.Interfaces.Repositories;
using Domain.Constants;
using Domain.Exceptions;

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

    public async Task<Rate> GetLastByCurrencyIdAsync(Guid currencyId)
    {
        Rate rate = await _context.Rates
            .AsNoTracking()
            .Where(x => x.CurrencyId == currencyId)
            .OrderByDescending(x => x.Date)
            .AsSplitQuery()
            .FirstOrDefaultAsync() ?? throw new EntityNotFoundException<Rate>();
         
        return rate;
    }

    public async Task<IEnumerable<Rate>> GetRangeAsync(DateTime date)
    {
        return await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date == date.Date)
            .OrderBy(x => x.Date)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end)
    {
        return await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date >= start.Date && x.Date.Date <= end.Date)
            .OrderBy(x => x.Date)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end, Currency currency)
    {
        List<Rate> rates = await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date >= start.Date && x.Date.Date <= end.Date && x.CurrencyId == currency.Id)
            .OrderBy(x => x.Date)
            .AsSplitQuery()
            .ToListAsync();

        return rates;
    }

    public async Task<IEnumerable<Rate>> GetRangeAsync(DateTime start, DateTime end, int page, int pageSize)
    {
        List<Rate> rates = await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date >= start.Date && x.Date.Date <= end.Date)
            .OrderBy(x => x.Date)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return rates;
    }

    public async Task<IEnumerable<Rate>> GetRangeAsync(
        DateTime start,
        DateTime end,
        Currency currency,
        int page,
        int pageSize)
    {
        List<Rate> rates = await _context.Rates
            .AsNoTracking()
            .Include(x => x.Currency)
            .Where(x => x.Date.Date >= start.Date && x.Date.Date <= end.Date && x.CurrencyId == currency.Id)
            .OrderBy(x => x.Date)
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