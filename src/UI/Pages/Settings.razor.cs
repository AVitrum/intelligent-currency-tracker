using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Payload.Responses;
using UI.Common.Interfaces;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Settings : ComponentBase, IPageComponent
{
    [Inject] private IUserSettingsService UserSettingsService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private HttpClient Http { set; get; } = null!;

    private SettingsDto Dto { get; set; } = new SettingsDto();

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
        if (await UserSettingsService.IsSettingsSetAsync(Js))
        {
            Dto = await UserSettingsService.GetSettingsAsync(Js);
        }
        else
        {
            Dto = new SettingsDto
            {
                Theme = "default",
                Language = "en-US",
                SummaryType = null,
                NotificationsEnabled = true
            };
        }
    }

    private async Task SaveSettings()
    {
        bool success = await UserSettingsService.SaveSettingsAsync(Dto, Js);
        if (!success)
        {
            ToastService.ShowError("Failed to save settings.");
            return;
        }

        string url =
            $"{Configuration.ApiUrl}/Identity/save-settings";

        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.PostAsJsonAsync(url, Dto));
            SaveSettingsResponse? response =
                await resp.Content.ReadFromJsonAsync<SaveSettingsResponse>();

            if (resp.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess("Settings saved successfully!");
                await Task.Delay(2000);
                await Js.InvokeVoidAsync("eval", "location.reload()");
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error: {ex.Message}");
        }
    }
}