using Microsoft.JSInterop;
using Shared.Dtos;

namespace UI.Common.Interfaces;

public interface IUserSettingsService
{
    Task<bool> SaveSettingsAsync(SettingsDto settings, IJSRuntime js);
    Task<SettingsDto> GetSettingsAsync(IJSRuntime js);
    Task<SettingsDto> SetDefaultSettingsAsync(IJSRuntime js);
    Task<bool> IsSettingsSetAsync(IJSRuntime js);
}