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

public partial class AllUsers : ComponentBase, IPageComponent
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;

    private readonly List<UserDto> _users = [];
    private int _currentPage = 1;
    private const int PageSize = 25;
    private bool _isLoading;

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
        await LoadUsers();
    }

    private async Task LoadUsers()
    {
        _isLoading = true;

        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.GetAsync($"{DevUISettings.ApiUrl}/Identity/get-all-users?page={_currentPage}&pageSize={PageSize}")
        );

        if (response.IsSuccessStatusCode)
        {
            GetAllUsersResponse? getAllUsersResponse = await response.Content.ReadFromJsonAsync<GetAllUsersResponse>();
            if (getAllUsersResponse?.Users is not null)
            {
                _users.AddRange(getAllUsersResponse.Users);
                _currentPage++;
            }
            else
            {
                string errorMessage = await HandleResponse(getAllUsersResponse);
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

    private async Task LoadMoreUsers()
    {
        await LoadUsers();
    }

    private void NavigateToUserDetails(string userId)
    {
        Navigation.NavigateTo($"/user-details/{userId}");
    }
}