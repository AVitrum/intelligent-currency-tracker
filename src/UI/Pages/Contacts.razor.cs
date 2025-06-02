using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Blazored.Toast.Services;
using Domain.Common;
using Shared.Payload.Responses;
using UI.Common.Interfaces;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Contacts : ComponentBase, IPageComponent
{
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private ReportFormModel ReportModel { get; } = new ReportFormModel();
    private List<IBrowserFile> SelectedFiles { get; } = [];
    private bool IsSubmitting { get; set; }

    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    private readonly string[] _allowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt"];

    public class ReportFormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string Description { get; set; } = string.Empty;
    }

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

    private void LoadFiles(InputFileChangeEventArgs e)
    {
        SelectedFiles.Clear();
        foreach (IBrowserFile file in e.GetMultipleFiles())
        {
            if (file.Size > MaxFileSize)
            {
                ToastService.ShowError($"File '{file.Name}' exceeds the maximum size of {FormatBytes(MaxFileSize)}.");
                continue;
            }

            string extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                ToastService.ShowError(
                    $"File type for '{file.Name}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
                continue;
            }

            SelectedFiles.Add(file);
        }
    }

    private string FormatBytes(long bytes, int decimals = 2)
    {
        if (bytes == 0)
        {
            return "0 Bytes";
        }

        const int k = 1024;
        string[] sizes = ["Bytes", "KB", "MB", "GB", "TB"];
        int i = Convert.ToInt32(Math.Floor(Math.Log(bytes) / Math.Log(k)));
        return $"{Math.Round(bytes / Math.Pow(k, i), decimals)} {sizes[i]}";
    }

    private async Task HandleSendReportAsync()
    {
        IsSubmitting = true;
        StateHasChanged();

        try
        {
            using MultipartFormDataContent content = new MultipartFormDataContent();
            content.Add(new StringContent(ReportModel.Title), "title");
            content.Add(new StringContent(ReportModel.Description), "description");

            foreach (IBrowserFile file in SelectedFiles)
            {
                StreamContent fileContent = new StreamContent(file.OpenReadStream(MaxFileSize));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "attachments", file.Name);
            }

            string apiUrl = $"{Configuration.ApiUrl}/Report/send";
            HttpResponseMessage response = await Http.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess(
                    "The report has been sent successfully! We will send you a response to your email as soon as possible.’");
                ReportModel.Title = string.Empty;
                ReportModel.Description = string.Empty;
                SelectedFiles.Clear();
                await Task.Delay(2000).ContinueWith(_ => NavigationManager.NavigateTo("/"));
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
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"An unexpected error occurred: {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }
}