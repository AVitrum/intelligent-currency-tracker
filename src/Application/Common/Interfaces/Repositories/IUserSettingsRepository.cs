using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces.Repositories;

public interface IUserSettingsRepository : IBaseRepository<UserSettings>
{
    Task AddOrUpdateAsync(UserSettings settings);
    Task<UserSettings?> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserSettings>> GetUserSettingsRangeBySummaryTypeAsync(SummaryType summaryType);
    Task<IEnumerable<UserSettings>> GetUserSettingsRangeByPercentageToNotifyAsync(decimal percentage);
}