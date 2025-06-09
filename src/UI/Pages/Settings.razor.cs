using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Payload.Responses;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Settings : ComponentBase, IPageComponent, IAsyncDisposable
{
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private HttpClient Http { set; get; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;

    private SettingsDto Dto { get; set; } = new SettingsDto();

    private string _pageTitle = "";
    private string _pageDescription = "";
    private string _languageLabel = "";
    private string _themeLabel = "";
    private string _summaryTypeLabel = "";
    private string _notificationsLabel = "";
    private string _saveButtonText = "";
    private string _toastFailedToSaveSettings = "";
    private string _toastSettingsSavedSuccessfully = "";
    private string _toastDefaultError = "";
    private string _errorOccurredProcessingRequest = "";


    public async Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return _errorOccurredProcessingRequest;
        }

        StringBuilder errorMessage = new StringBuilder();
        errorMessage.AppendLine(
            $"{await Localizer.GetStringAsync("settings.error.message_prefix")} {response.Message}");
        errorMessage.AppendLine(
            $"{await Localizer.GetStringAsync("settings.error.status_code_prefix")} {response.StatusCode}");

        if (response.Errors.Any())
        {
            errorMessage.AppendLine(
                $"{await Localizer.GetStringAsync("settings.error.errors_prefix")} {string.Join(", ", response.Errors)}");
        }

        return errorMessage.ToString();
    }

    public async Task HandleInvalidResponse(
        string? message = null)
    {
        ToastService.ShowError(message ?? _toastDefaultError);
        await Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;

        if (await UserSettingsService.IsSettingsSetAsync(Js))
        {
            Dto = await UserSettingsService.GetSettingsAsync(Js);
        }
        else
        {
            Dto = new SettingsDto
            {
                Theme = "default",
                Language = "en-US",
                SummaryType = null,
                NotificationsEnabled = true
            };
        }
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("settings.title");
        _pageDescription = await Localizer.GetStringAsync("settings.description");
        _languageLabel = await Localizer.GetStringAsync("settings.language_label");
        _themeLabel = await Localizer.GetStringAsync("settings.theme_label");
        _summaryTypeLabel = await Localizer.GetStringAsync("settings.summary_type_label");
        _notificationsLabel = await Localizer.GetStringAsync("settings.notifications_label");
        _saveButtonText = await Localizer.GetStringAsync("settings.save_button");
        _toastFailedToSaveSettings = await Localizer.GetStringAsync("settings.toast.failed_to_save");
        _toastSettingsSavedSuccessfully = await Localizer.GetStringAsync("settings.toast.saved_successfully");
        _toastDefaultError = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorOccurredProcessingRequest = await Localizer.GetStringAsync("settings.error.generic_processing");
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    private async Task SaveSettings()
    {
        bool success = await UserSettingsService.SaveSettingsAsync(Dto, Js);
        if (!success)
        {
            ToastService.ShowError(_toastFailedToSaveSettings);
            return;
        }

        string url =
            $"{Configuration.ApiUrl}/Identity/save-settings";

        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.PostAsJsonAsync(url, Dto));
            SaveSettingsResponse? response =
                await resp.Content.ReadFromJsonAsync<SaveSettingsResponse>();

            if (resp.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess(_toastSettingsSavedSuccessfully);
                await Task.Delay(2000);
                await Js.InvokeVoidAsync("eval", "location.reload()");
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            string localizedErrorPrefix = await Localizer.GetStringAsync("settings.error.exception_prefix");
            await HandleInvalidResponse($"{localizedErrorPrefix} {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
    }
}