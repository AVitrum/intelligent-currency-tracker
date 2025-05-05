using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Shared.Payload.Requests;

namespace UI.Pages;

public partial class ChangePassword : ComponentBase, IPageComponent
{
    private ChangePasswordRequest Request { get; set; } = new ChangePasswordRequest();

    [Inject]
    protected HttpClient Http { get; set; } = null!;

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

    public Task HandleInvalidResponse(string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);
        return Task.CompletedTask;
    }

    protected async Task HandleValidSubmit()
    {
        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.PostAsJsonAsync($"{UISettings.ApiUrl}/Identity/change-password", Request));
        
        if (!response.IsSuccessStatusCode)
        {
            string errorResponse = await HandleResponse(response);
            await HandleInvalidResponse(errorResponse);
        }
        else
        {
            ToastService.ShowSuccess("Password changed successfully.");
            ResetForm();
        }
    }

    private void ResetForm()
    {
        Request = new ChangePasswordRequest();
    }
}