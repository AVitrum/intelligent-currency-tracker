namespace Shared.Dtos;

public class SettingsDto
{
    public string Language { get; set; } = nameof(Domain.Enums.Language.En);
    public string Theme { get; set; } = nameof(Domain.Enums.Theme.Light);
    public bool NotificationsEnabled { get; set; } = true;
}