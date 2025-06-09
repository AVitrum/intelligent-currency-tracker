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
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;


namespace UI.Pages;

public partial class Contacts : ComponentBase, IPageComponent, IAsyncDisposable
{
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private ReportFormModel ReportModel { get; } = new ReportFormModel();
    private List<IBrowserFile> SelectedFiles { get; } = [];
    private bool IsSubmitting { get; set; }

    private const long MaxFileSize = 10 * 1024 * 1024;

    private readonly string[] _allowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt"];

    private string _pageTitle = "";
    private string _headerTitle = "";
    private string _headerDescription = "";
    private string _formCardTitle = "";
    private string _labelTitle = "";
    private string _placeholderTitle = "";
    private string _labelDescription = "";
    private string _placeholderDescription = "";
    private string _labelAttachments = "";
    private string _buttonSendReport = "";
    private string _buttonSending = "";
    private string _toastReportSentSuccessfully = "";
    private string _toastFileTooLargeFormat = "";
    private string _toastInvalidFileExtensionFormat = "";
    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorResponseNull = "";


    public class ReportFormModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters.")]
        public string Description { get; set; } = string.Empty;
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("contacts.page_title");
        _headerTitle = await Localizer.GetStringAsync("contacts.header.title");
        _headerDescription = await Localizer.GetStringAsync("contacts.header.description");
        _formCardTitle = await Localizer.GetStringAsync("contacts.form.card_title");
        _labelTitle = await Localizer.GetStringAsync("contacts.form.label.title");
        _placeholderTitle = await Localizer.GetStringAsync("contacts.form.placeholder.title");
        _labelDescription = await Localizer.GetStringAsync("contacts.form.label.description");
        _placeholderDescription = await Localizer.GetStringAsync("contacts.form.placeholder.description");
        _labelAttachments = await Localizer.GetStringAsync("contacts.form.label.attachments");
        _buttonSendReport = await Localizer.GetStringAsync("contacts.form.button.send_report");
        _buttonSending = await Localizer.GetStringAsync("contacts.form.button.sending");
        _toastReportSentSuccessfully = await Localizer.GetStringAsync("contacts.toast.report_sent_successfully");
        _toastFileTooLargeFormat = await Localizer.GetStringAsync("contacts.toast.file_too_large_format");
        _toastInvalidFileExtensionFormat =
            await Localizer.GetStringAsync("contacts.toast.invalid_file_extension_format");

        _errorGeneric = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
        _errorResponseNull = await Localizer.GetStringAsync("contacts.error.response_null");
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult(_errorResponseNull);
        }

        StringBuilder errorMessage = new StringBuilder();
        errorMessage.AppendLine($"{_errorMessagePrefix} {response.Message}");
        errorMessage.AppendLine($"{_errorStatusCodePrefix} {response.StatusCode}");

        if (response.Errors.Any())
        {
            errorMessage.AppendLine($"{_errorErrorsPrefix} {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessage.ToString());
    }

    public Task HandleInvalidResponse(string? message = null)
    {
        ToastService.ShowError(message ?? _errorGeneric);
        return Task.CompletedTask;
    }

    private void LoadFiles(InputFileChangeEventArgs e)
    {
        SelectedFiles.Clear();
        foreach (IBrowserFile file in e.GetMultipleFiles())
        {
            if (file.Size > MaxFileSize)
            {
                ToastService.ShowError(string.Format(_toastFileTooLargeFormat, file.Name, FormatBytes(MaxFileSize)));
                continue;
            }

            string extension = Path.GetExtension(file.Name).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                ToastService.ShowError(string.Format(_toastInvalidFileExtensionFormat, file.Name,
                    string.Join(", ", _allowedExtensions)));
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
            HttpResponseMessage response =
                await HttpClientService.SendRequestAsync(() => Http.PostAsync(apiUrl, content));

            if (response.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess(_toastReportSentSuccessfully);
                ReportModel.Title = string.Empty;
                ReportModel.Description = string.Empty;
                SelectedFiles.Clear();
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
            await HandleInvalidResponse($"{_errorGeneric} {ex.Message}");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}