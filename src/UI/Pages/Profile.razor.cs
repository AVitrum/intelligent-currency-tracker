using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Shared.Payload.Responses;

namespace UI.Pages;

public partial class Profile : ComponentBase, IPageComponent
{
    private ProfileResponse? _profile;
    private string _profileImageSrc = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        string url = $"{UISettings.ApiUrl}/Identity/profile";
        try
        {
            HttpResponseMessage response = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            if (response.IsSuccessStatusCode)
            {
                _profile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
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

    public Task HandleInvalidResponse(string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);
        return Task.CompletedTask;
    }

    public async Task<string> HandleResponse(HttpResponseMessage response)
    {
        string errorResponse = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(errorResponse))
        {
            return "Something went wrong. Please try again.";
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(errorResponse);
            if (doc.RootElement.TryGetProperty("title", out JsonElement titleElement))
            {
                return titleElement.GetString() ?? errorResponse;
            }

            if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement))
            {
                return messageElement.GetString() ?? errorResponse;
            }

            return errorResponse;
        }
        catch (JsonException)
        {
            return errorResponse;
        }
    }
}