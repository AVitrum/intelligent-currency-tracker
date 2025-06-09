using Microsoft.JSInterop;
using Shared.Dtos;

namespace UI.Services;

public class UserSettingsService
{
    private const int ExpirationDays = 7;

    public event Func<Task>? OnSettingsChangedAsync;

    public async Task<bool> SaveSettingsAsync(SettingsDto settings, IJSRuntime js)
    {
        await ApplySettingsAsync(settings, js);
        return true;
    }

    public async Task<SettingsDto> GetSettingsAsync(IJSRuntime js)
    {
        await EnsureJsFunctionsExistAsync(js);
        string language = await js.InvokeAsync<string>("getCookie", "language");
        string theme = await js.InvokeAsync<string>("getCookie", "theme");
        string? summaryType = await js.InvokeAsync<string?>("getCookie", "summaryType");
        string notificationsEnabled = await js.InvokeAsync<string>("getCookie", "notificationsEnabled");

        if (string.IsNullOrEmpty(language) || string.IsNullOrEmpty(theme) ||
            string.IsNullOrEmpty(notificationsEnabled))
        {
            return await SetDefaultSettingsAsync(js);
        }

        return new SettingsDto
        {
            Language = language,
            Theme = theme,
            SummaryType = summaryType,
            NotificationsEnabled = bool.Parse(notificationsEnabled)
        };
    }

    public async Task<bool> IsSettingsSetAsync(IJSRuntime js)
    {
        await EnsureJsFunctionsExistAsync(js);
        string language = await js.InvokeAsync<string>("getCookie", "language");
        string theme = await js.InvokeAsync<string>("getCookie", "theme");
        string notificationsEnabled = await js.InvokeAsync<string>("getCookie", "notificationsEnabled");

        return !string.IsNullOrEmpty(language) && !string.IsNullOrEmpty(theme) &&
               !string.IsNullOrEmpty(notificationsEnabled);
    }

    public async Task<SettingsDto> SetDefaultSettingsAsync(IJSRuntime js)
    {
        SettingsDto defaultSettings = new SettingsDto();
        await ApplySettingsAsync(defaultSettings, js);
        return defaultSettings;
    }

    private async Task ApplySettingsAsync(SettingsDto settings, IJSRuntime js)
    {
        await EnsureJsFunctionsExistAsync(js);
        await js.InvokeVoidAsync("setCookie", "language", settings.Language, ExpirationDays);
        await js.InvokeVoidAsync("setCookie", "theme", settings.Theme, ExpirationDays);
        await js.InvokeVoidAsync("setCookie", "notificationsEnabled", settings.NotificationsEnabled.ToString(),
            ExpirationDays);

        if (!string.IsNullOrEmpty(settings.SummaryType))
        {
            await js.InvokeVoidAsync("setCookie", "summaryType", settings.SummaryType, ExpirationDays);
        }
        else
        {
            await js.InvokeVoidAsync("deleteCookie", "summaryType");
        }

        if (OnSettingsChangedAsync != null)
        {
            await OnSettingsChangedAsync.Invoke();
        }
    }

    private async Task EnsureJsFunctionsExistAsync(IJSRuntime js)
    {
        await js.InvokeVoidAsync("eval",
            """

                        window.setCookie = function(name, value, days) {
                            let expires = '';
                            if (days) {
                                let date = new Date();
                                date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
                                expires = '; expires=' + date.toUTCString();
                            }
                            document.cookie = name + '=' + value + expires + '; path=/';
                        };

                        window.getCookie = function(name) {
                            let nameEQ = name + '=';
                            let ca = document.cookie.split(';');
                            for (let i = 0; i < ca.length; i++) {
                                let c = ca[i];
                                while (c.charAt(0) == ' ') c = c.substring(1, c.length);
                                if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
                            }
                            return null;
                        };

                        window.deleteCookie = function(name) {
                            document.cookie = name + '=; Expires=Thu, 01 Jan 1970 00:00:01 GMT; Path=/;';
                        };
                    
            """);
    }
}