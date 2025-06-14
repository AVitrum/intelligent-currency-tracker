using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class UserSettings : BaseEntity
{
    public Language Language { get; set; } = Language.En;
    public Theme Theme { get; set; } = Theme.Light;
    public SummaryType? SummaryType { get; set; }
    public decimal PercentageToNotify { get; set; } = 100;
    public bool NotificationsEnabled { get; set; } = true;

    public required string UserId { get; set; }
}