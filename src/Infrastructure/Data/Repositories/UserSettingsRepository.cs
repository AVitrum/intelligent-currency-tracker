using Application.Common.Interfaces.Repositories;

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
}