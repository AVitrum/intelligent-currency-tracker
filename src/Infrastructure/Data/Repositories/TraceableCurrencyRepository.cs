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

    public async Task<IEnumerable<TraceableCurrency>> GetByUserIdAsync(string userId)
    {
        return await _context.TraceableCurrencies
            .Where(tc => tc.UserId == userId)
            .Include(tc => tc.Currency)
            .ToListAsync();
    }
}