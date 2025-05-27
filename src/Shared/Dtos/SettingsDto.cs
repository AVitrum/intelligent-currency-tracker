namespace Shared.Dtos;

public class SettingsDto
{
    public string Language { get; set; } = nameof(Domain.Enums.Language.En);
    public string Theme { get; set; } = nameof(Domain.Enums.Theme.Light);
    public string PercentageToNotify { get; set; } = 100.ToString();
    public string? SummaryType { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
}