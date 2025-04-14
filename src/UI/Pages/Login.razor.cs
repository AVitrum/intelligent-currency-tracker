using System.Net.Http.Json;
using Domain.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Helpers;
using Shared.Payload.Requests;
using Shared.Payload.Responses;

namespace UI.Pages;

public partial class Login : ComponentBase, IPageComponent
{
    private readonly LoginRequest _request = new LoginRequest { Provider = LoginManagerProvider.Default.ToString() };

    [Inject] private IJSRuntime Js { get; set; } = null!;
    private string? _errorMessage;

    private async Task HandleValidSubmit()
    {
        if (_request.Email is not null && !_request.Email.Contains("@"))
        {
            _request.UserName = _request.Email;
            _request.Email = string.Empty;
        }

        try
        {
            HttpResponseMessage response = await Http.PostAsJsonAsync($"{UISettings.ApiUrl}/Identity/login", _request);
            if (response.IsSuccessStatusCode)
            {
                LoginResponse? responseContent = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (responseContent?.Token is not null)
                {
                    await JwtTokenHelper.SetJwtTokensInCookiesAsync(responseContent.Token, responseContent.RefreshToken, Js);
                    ToastService.ShowSuccess("User successfully login!");
                    Navigation.NavigateTo("/all-users", true);
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
    
    public async Task HandleInvalidResponse(string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);
        
        await Task.Delay(3000);
        _errorMessage = null;
        _request.Email = string.Empty;
        _request.UserName = string.Empty;
        _request.Password = string.Empty;
    }
}