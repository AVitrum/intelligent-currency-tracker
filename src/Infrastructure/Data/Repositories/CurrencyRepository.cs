using Application.Common.Interfaces.Repositories;

namespace Infrastructure.Data.Repositories;

public class CurrencyRepository : BaseRepository<Currency>, ICurrencyRepository
{
    private readonly ApplicationDbContext _context;

    public CurrencyRepository(ApplicationDbContext context) : base(context, context.Currencies)
    {
        _context = context;
    }

    public async Task<Currency?> GetByCodeAsync(string code)
    {
        return await _context.Currencies.FirstOrDefaultAsync(currency => currency.Code == code);
    }
}