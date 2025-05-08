using System.Net.Http.Json;
using System.Text;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Identity;

namespace UI.Pages;

public partial class ChangePassword : ComponentBase, IPageComponent
{
    private ChangePasswordRequest Request { get; set; } = new ChangePasswordRequest();

    [Inject]
    protected HttpClient Http { get; set; } = null!;

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

    private async Task HandleValidSubmit()
    {
        HttpResponseMessage res = await HttpClientService.SendRequestAsync(() =>
            Http.PostAsJsonAsync($"{UISettings.ApiUrl}/Identity/change-password", Request));
        ChangePasswordResponse? response = await res.Content.ReadFromJsonAsync<ChangePasswordResponse>();

        if (!res.IsSuccessStatusCode)
        {
            string errorResponse = await HandleResponse(response);
            await HandleInvalidResponse(errorResponse);
        }
        else
        {
            ToastService.ShowSuccess(response!.Message);
            ResetForm();
        }
    }

    private void ResetForm()
    {
        Request = new ChangePasswordRequest();
    }
}