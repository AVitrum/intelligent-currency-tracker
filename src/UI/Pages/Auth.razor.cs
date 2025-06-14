using System.Net.Http.Json;
using System.Text;
using Domain.Common;
using Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Helpers;
using Shared.Payload.Requests;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Identity;
using UI.Common.Interfaces;

namespace UI.Pages;

public partial class Auth : ComponentBase, IPageComponent
{
    private readonly LoginRequest _loginRequest = new LoginRequest { Provider = nameof(LoginManagerProvider.Default) };

    private readonly CreateUserDto _registrationRequest = new CreateUserDto
        { Provider = nameof(UserServiceProvider.DEFAULT) };

    private string? _errorMessage;
    private bool IsLogin { get; set; } = true;
    
    [Inject] private IJSRuntime Js { get; set; } = null!;

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult("An error occurred while processing your request. Try again later.");
        }

        StringBuilder errorMessage = new StringBuilder();

        errorMessage.AppendLine($"Message: {response.Message}");
        errorMessage.AppendLine($"Status Code: {response.StatusCode}");

        if (response.Errors.Any())
        {
            errorMessage.AppendLine($"Errors: {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessage.ToString());
    }

    public Task HandleInvalidResponse(
        string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);

        _errorMessage = null;
        _loginRequest.Identifier = string.Empty;
        _loginRequest.Password = string.Empty;
        return Task.CompletedTask;
    }
    
    protected override async Task OnInitializedAsync()
    {
        if (!await IsTokenPresentAsync())
        {
            Navigation.NavigateTo("/dashboard", forceLoad: true);
        }
        await base.OnInitializedAsync();
    }

    private async Task HandleLoginValidSubmit()
    {
        try
        {
            HttpResponseMessage res =
                await Http.PostAsJsonAsync($"{Configuration.ApiUrl}/Identity/login", _loginRequest);
            LoginResponse? response = await res.Content.ReadFromJsonAsync<LoginResponse>();

            if (res.IsSuccessStatusCode)
            {
                await JwtTokenHelper.SetJwtTokensInCookiesAsync(response!.Token, response.RefreshToken, Js);
                await GetSettings();

                ToastService.ShowSuccess("User successfully login!");
                Navigation.NavigateTo("/", true);
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch
        {
            await HandleInvalidResponse();
        }
    }

    private async Task HandleRegistrationValidSubmit()
    {
        HttpResponseMessage res =
            await Http.PostAsJsonAsync($"{Configuration.ApiUrl}/Identity/register", _registrationRequest);
        RegistrationResponse? response = await res.Content.ReadFromJsonAsync<RegistrationResponse>();

        if (res.IsSuccessStatusCode)
        {
            ToastService.ShowSuccess("User successfully registered!");
            Navigation.NavigateTo("/auth", true);
        }
        else
        {
            string err = await HandleResponse(response);
            await HandleInvalidResponse(err);
        }
    }

    private async Task GetSettings()
    {
        string url =
            $"{Configuration.ApiUrl}/Identity/get-settings";

        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetSettingsResponse? response =
                await resp.Content.ReadFromJsonAsync<GetSettingsResponse>();

            if (resp.IsSuccessStatusCode)
            {
                await UserSettingsService.SaveSettingsAsync(response!.Settings!, Js);
            }
            else
            {
                await UserSettingsService.SetDefaultSettingsAsync(Js);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error: {ex.Message}");
        }
    }

    private async Task<bool> IsTokenPresentAsync()
    {
        string? token = await JwtTokenHelper.GetJwtTokenFromCookies(Js);
        return string.IsNullOrEmpty(token);
    }
    
    private void ToggleLoginMode()
    {
        IsLogin = !IsLogin;
    }
}