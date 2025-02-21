using Application.Common.Interfaces.Repositories;
using Infrastructure.Identity;

namespace Infrastructure.Utils;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByUserIdAsync(string userId);
}