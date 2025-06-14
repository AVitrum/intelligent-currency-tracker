using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using DevUI.Common.Interfaces;
using DevUI.Interfaces;
using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Identity;

namespace DevUI.Pages;

public partial class UserDetails : ComponentBase, IPageComponent
{
    [Parameter] public string? UserId { get; set; }

    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;

    private UserDto? _user;
    private bool _isLoading = true;

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

    protected override async Task OnInitializedAsync()
    {
        await LoadUserDetails();
    }

    private async Task LoadUserDetails()
    {
        _isLoading = true;

        HttpResponseMessage? response = await HttpClientService.SendRequestAsync(() =>
            Http.GetAsync($"{DevUISettings.ApiUrl}/Identity/get-user-by-id?id={UserId}")
        );

        if (response.IsSuccessStatusCode)
        {
            GetUserResponse? getUserResponse = await response.Content.ReadFromJsonAsync<GetUserResponse>();
            if (getUserResponse?.User is not null)
            {
                _user = getUserResponse.User;
            }
            else
            {
                string errorMessage = await HandleResponse(getUserResponse);
                await HandleInvalidResponse(errorMessage);
            }
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
        }

        _isLoading = false;
    }

    private void NavigateBack()
    {
        Navigation.NavigateTo("/all-users");
    }
}