using Infrastructure.Identity;
using Infrastructure.Utils;

namespace Infrastructure.Data.Repositories;

public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context) : base(context, context.RefreshTokens)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId);
    }
}