using Application.Common.Interfaces.Repositories;
using Domain.Enums;

namespace Infrastructure.Data.Repositories;

public class UserSettingsRepository : BaseRepository<UserSettings>, IUserSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public UserSettingsRepository(ApplicationDbContext context) : base(context, context.UserSettings)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(UserSettings settings)
    {
        UserSettings? existingSettings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == settings.UserId);

        if (existingSettings is not null)
        {
            existingSettings.Language = settings.Language;
            existingSettings.Theme = settings.Theme;
            existingSettings.SummaryType = settings.SummaryType;
            existingSettings.NotificationsEnabled = settings.NotificationsEnabled;
        }
        else
        {
            await _context.UserSettings.AddAsync(settings);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserSettings?> GetByUserIdAsync(string userId)
    {
        return await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId);
    }

    public async Task<IEnumerable<UserSettings>> GetUserSettingsRangeBySummaryTypeAsync(SummaryType summaryType)
    {
        return await _context.UserSettings
            .Where(us => us.SummaryType == summaryType && us.NotificationsEnabled)
            .OrderBy(us => us.TimeStamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserSettings>> GetUserSettingsRangeByPercentageToNotifyAsync(decimal percentage)
    {
        List<UserSettings> userSettings = await _context.UserSettings
            .Where(us => us.PercentageToNotify <= percentage && us.NotificationsEnabled)
            .OrderBy(us => us.TimeStamp)
            .ToListAsync();

        return userSettings;
    }
}