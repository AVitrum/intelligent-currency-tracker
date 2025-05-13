using System.Net.Http.Json;
using System.Text;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Shared.Payload.Responses.Identity;
using UI.Common.Interfaces;

namespace UI.Pages;

public partial class Profile : ComponentBase, IPageComponent
{
    private ProfileResponse? _profile;
    private string _profileImageSrc = string.Empty;

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
        return Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadProfileAsync();
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
            await HandleInvalidResponse($"Error: {ex.Message}");
        }
    }

    private void OnPasswordChange()
    {
        Navigation.NavigateTo("/change-password");
    }
}