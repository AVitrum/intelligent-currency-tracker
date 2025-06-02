using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using DevUI.Common.Interfaces;
using DevUI.Interfaces;
using Domain.Common;
using Domain.Enums;
using Shared.Dtos;
using Shared.Payload.Responses;

namespace DevUI.Pages;

public partial class CreateAdmin : ComponentBase, IPageComponent
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;

    private readonly CreateUserDto _request = new CreateUserDto();

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult("An error occurred while processing your request.");
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
        return Task.CompletedTask;
    }

    private async Task HandleValidSubmit()
    {
        _request.Provider = nameof(UserServiceProvider.ADMIN);

        HttpResponseMessage? response = await HttpClientService.SendRequestAsync(() =>
            Http.PostAsJsonAsync($"{DevUISettings.ApiUrl}/Identity/create-admin", _request)
        );

        if (response.IsSuccessStatusCode)
        {
            ToastService.ShowSuccess("Admin successfully created!");
            Navigation.NavigateTo("/");
        }
        else
        {
            DefaultResponse? errorResponse = null;
            try
            {
                errorResponse = await response.Content.ReadFromJsonAsync<DefaultResponse>();
            }
            catch (Exception)
            {
                // ignored
            }

            string errorMessage = await HandleResponse(errorResponse);
            await HandleInvalidResponse(errorMessage);

            await Task.Delay(3000);
            _request.Email = string.Empty;
            _request.UserName = string.Empty;
            _request.Password = string.Empty;
            _request.PhoneNumber = string.Empty;
        }
    }
}