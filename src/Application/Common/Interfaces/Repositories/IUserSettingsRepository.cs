using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IUserSettingsRepository : IBaseRepository<UserSettings>
{
    Task AddOrUpdateAsync(UserSettings settings);
    Task<UserSettings?> GetByUserIdAsync(string userId);
}