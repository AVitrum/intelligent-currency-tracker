using Domain.Exceptions;
using Infrastructure.Identity.SubUserEntities;
using Infrastructure.Interfaces;

namespace Infrastructure.Data.Repositories;

public class TraceableCurrencyRepository : BaseRepository<TraceableCurrency>, ITraceableCurrencyRepository
{
    private readonly ApplicationDbContext _context;

    public TraceableCurrencyRepository(ApplicationDbContext context) : base(context, context.TraceableCurrencies)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string userId, Guid currencyId)
    {
        return await _context.TraceableCurrencies
            .AnyAsync(tc => tc.UserId == userId && tc.CurrencyId == currencyId);
    }

    public async Task<IEnumerable<TraceableCurrency>> GetByUserIdAsync(string userId)
    {
        return await _context.TraceableCurrencies
            .Where(tc => tc.UserId == userId)
            .Include(tc => tc.Currency)
            .ToListAsync();
    }

    public async Task RemoveAsync(string userId, Guid currencyId)
    {
        TraceableCurrency traceableCurrency = await _context.TraceableCurrencies
                                                  .FirstOrDefaultAsync(tc =>
                                                      tc.UserId == userId && tc.CurrencyId == currencyId)
                                              ?? throw new EntityNotFoundException<TraceableCurrency>();

        _context.TraceableCurrencies.Remove(traceableCurrency);
        await _context.SaveChangesAsync();
    }
}