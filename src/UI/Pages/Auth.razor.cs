using System.Net.Http.Json;
using Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Helpers;
using Shared.Payload.Requests;
using Shared.Payload.Responses;

namespace UI.Pages;

public partial class Auth : ComponentBase, IPageComponent
{
    private readonly LoginRequest _loginRequest = new LoginRequest { Provider = nameof(LoginManagerProvider.Default) };

    private readonly CreateUserDto _registrationRequest = new CreateUserDto
        { Provider = nameof(UserServiceProvider.DEFAULT) };

    private string? _errorMessage;
    private bool IsLogin { get; set; } = true;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    public async Task HandleInvalidResponse(
        string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);

        await Task.Delay(3000);
        _errorMessage = null;
        _loginRequest.Email = string.Empty;
        _loginRequest.UserName = string.Empty;
        _loginRequest.Password = string.Empty;
    }

    private async Task HandleLoginValidSubmit()
    {
        if (_loginRequest.Email is not null && !_loginRequest.Email.Contains('@'))
        {
            _loginRequest.UserName = _loginRequest.Email;
            _loginRequest.Email = string.Empty;
        }

        try
        {
            HttpResponseMessage response =
                await Http.PostAsJsonAsync($"{UISettings.ApiUrl}/Identity/login", _loginRequest);
            if (response.IsSuccessStatusCode)
            {
                LoginResponse? responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (responseContent?.Token is not null)
                {
                    await JwtTokenHelper.SetJwtTokensInCookiesAsync(responseContent.Token, responseContent.RefreshToken,
                        Js);
                    ToastService.ShowSuccess("User successfully login!");
                    Navigation.NavigateTo("/", true);
                }
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                string errorMessage = !string.IsNullOrEmpty(errorResponse)
                    ? errorResponse
                    : "Login failed. Please try again.";

                await HandleInvalidResponse(errorMessage);
            }
        }
        catch (Exception)
        {
            await HandleInvalidResponse();
        }
    }

    private async Task HandleRegistrationValidSubmit()
    {
        HttpResponseMessage response = await Http.PostAsJsonAsync(
            $"{UISettings.ApiUrl}/Identity/register", _registrationRequest);

        if (response.IsSuccessStatusCode)
        {
            ToastService.ShowSuccess("User successfully registered!");
            Navigation.NavigateTo("/login", true);
        }
        else
        {
            string errorResponse = await response.Content.ReadAsStringAsync();
            string errorMessage = !string.IsNullOrEmpty(errorResponse)
                ? errorResponse
                : "Registration failed. Please try again.";

            await HandleInvalidResponse(errorMessage);
        }
    }

    private void ToggleLoginMode()
    {
        IsLogin = !IsLogin;
    }
}