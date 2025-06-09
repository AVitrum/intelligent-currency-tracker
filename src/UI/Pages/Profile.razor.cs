using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Shared.Payload.Responses.Identity;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Profile : ComponentBase, IPageComponent, IAsyncDisposable
{
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private ProfileResponse? _profile;
    private string _profileImageSrc = string.Empty;

    private string _pageTitle = "";
    private string _headerTitle = "";
    private string _profilePhotoAlt = "";
    private string _noPhotoAvailable = "";
    private string _labelName = "";
    private string _labelEmail = "";
    private string _labelPhone = "";
    private string _buttonModifyInformation = "";
    private string _buttonChangePassword = "";
    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorExceptionPrefix = "";


    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
        await LoadProfileAsync();
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("profile.page_title");
        _headerTitle = await Localizer.GetStringAsync("profile.header_title");
        _profilePhotoAlt = await Localizer.GetStringAsync("profile.photo_alt");
        _noPhotoAvailable = await Localizer.GetStringAsync("profile.no_photo");
        _labelName = await Localizer.GetStringAsync("profile.label.name");
        _labelEmail = await Localizer.GetStringAsync("profile.label.email");
        _labelPhone = await Localizer.GetStringAsync("profile.label.phone");
        _buttonModifyInformation = await Localizer.GetStringAsync("profile.button.modify_info");
        _buttonChangePassword = await Localizer.GetStringAsync("profile.button.change_password");
        _errorGeneric =
            await Localizer.GetStringAsync("settings.toast.default_error_try_again"); // Reusing generic error
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
        _errorExceptionPrefix = await Localizer.GetStringAsync("settings.error.exception_prefix");
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult(_errorGeneric);
        }

        StringBuilder errorMessage = new StringBuilder();
        errorMessage.AppendLine($"{_errorMessagePrefix} {response.Message}");
        errorMessage.AppendLine($"{_errorStatusCodePrefix} {response.StatusCode}");

        if (response.Errors.Any())
        {
            errorMessage.AppendLine($"{_errorErrorsPrefix} {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessage.ToString());
    }

    public async Task HandleInvalidResponse(string? message = null)
    {
        ToastService.ShowError(message ?? _errorGeneric);
        await Task.CompletedTask;
    }

    private async Task LoadProfileAsync()
    {
        string url = $"{Configuration.ApiUrl}/Identity/profile";
        try
        {
            HttpResponseMessage res = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            ProfileResponse? response = await res.Content.ReadFromJsonAsync<ProfileResponse>();

            if (res.IsSuccessStatusCode)
            {
                _profile = response;
                if (_profile?.Photo is { Length: > 0 })
                {
                    string base64Image = Convert.ToBase64String(_profile.Photo);
                    _profileImageSrc = $"data:image/jpeg;base64,{base64Image}";
                }
            }
            else
            {
                string errorResponse = await HandleResponse(response);
                await HandleInvalidResponse(errorResponse);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"{_errorExceptionPrefix} {ex.Message}");
        }
    }

    private void OnPasswordChange()
    {
        Navigation.NavigateTo("/change-password");
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
    }
}