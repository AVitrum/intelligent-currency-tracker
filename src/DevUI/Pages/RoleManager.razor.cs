using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using DevUI.Common.Interfaces;
using DevUI.Interfaces;
using Domain.Common;
using Domain.Enums;
using Shared.Payload.Requests;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Identity;

namespace DevUI.Pages;

public partial class RoleManager : ComponentBase, IPageComponent
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

    private readonly ChangeRoleRequest _roleChangeRequest = new ChangeRoleRequest
        { RoleString = nameof(UserRole.USER) };

    private List<string> _emailSuggestions = [];

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

    private async Task HandleRoleChange()
    {
        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.PostAsJsonAsync($"{DevUISettings.ApiUrl}/Identity/change-role", _roleChangeRequest)
        );

        if (response.IsSuccessStatusCode)
        {
            ToastService.ShowSuccess("Role assigned successfully");
            Navigation.NavigateTo("/role-manager", true);
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
            ResetRequestFields();
        }
    }

    private async Task SearchEmailSuggestions(ChangeEventArgs e)
    {
        string? searchTerm = e.Value?.ToString();
        if (string.IsNullOrEmpty(searchTerm))
        {
            _emailSuggestions.Clear();
            return;
        }

        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.GetAsync($"{DevUISettings.ApiUrl}/Identity/search-emails?query={searchTerm}"));

        if (response.IsSuccessStatusCode)
        {
            SearchEmailsResponse? searchEmailsResponse =
                await response.Content.ReadFromJsonAsync<SearchEmailsResponse>();
            _emailSuggestions = searchEmailsResponse?.Data.ToList() ?? [];
        }
        else
        {
            _emailSuggestions.Clear();
        }
    }

    private void SelectSuggestedEmail(string email)
    {
        _roleChangeRequest.Email = email;
        _emailSuggestions.Clear();
    }

    private void ResetRequestFields()
    {
        _roleChangeRequest.Email = string.Empty;
    }
}