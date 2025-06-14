using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using DevUI.Common.Interfaces;
using DevUI.Interfaces;
using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Report;

namespace DevUI.Pages;

public partial class ReportDetails : ComponentBase, IPageComponent
{
    [Parameter] public Guid ReportId { get; set; }

    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;

    private ReportDto? _report;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    private bool _isImageViewerOpen;
    private string? _currentImageInViewer; 

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult("An error occurred while processing your request.");
        }

        StringBuilder errorMessageBuilder = new StringBuilder();
        errorMessageBuilder.AppendLine($"Message: {response.Message}");
        errorMessageBuilder.AppendLine($"Status Code: {response.StatusCode}");
        if (response.Errors.Any()) 
        {
            errorMessageBuilder.AppendLine($"Errors: {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessageBuilder.ToString());
    }

    public Task HandleInvalidResponse(
        string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);
        _errorMessage = message;
        return Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (ReportId != Guid.Empty)
        {
            await LoadReportDetails();
        }
    }

    private async Task LoadReportDetails()
    {
        _isLoading = true;
        _report = null;
        _errorMessage = string.Empty;
        _isImageViewerOpen = false;
        _currentImageInViewer = null;
        StateHasChanged();

        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.GetAsync($"{DevUISettings.ApiUrl}/Report/get/{ReportId}")
        );

        if (response.IsSuccessStatusCode)
        {
            GetReportByIdResponse? reportResponse = await response.Content.ReadFromJsonAsync<GetReportByIdResponse>();
            if (reportResponse?.Report is not null)
            {
                _report = reportResponse.Report;
            }
            else
            {
                string errorMsg = await HandleResponse(reportResponse);
                await HandleInvalidResponse(errorMsg);
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

            string errorMsg = await HandleResponse(errorResponse);
            await HandleInvalidResponse(errorMsg);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _errorMessage = "Report not found.";
            }
        }

        _isLoading = false;
        StateHasChanged();
    }

    private string GetStatusClass(string? status)
    {
        return status?.ToLower() switch
        {
            "pending" => "status-pending",
            "inprogress" => "status-inprogress",
            "resolved" => "status-resolved",
            _ => "status-unknown"
        };
    }

    private async Task NavigateToReportList()
    {
        Navigation.NavigateTo("/all-reports");
        await Task.CompletedTask;
    }

    private void ViewImage(string imageUrl)
    {
        _currentImageInViewer = imageUrl;
        _isImageViewerOpen = true;
        StateHasChanged();
    }

    private void CloseImageViewer()
    {
        _isImageViewerOpen = false;
        _currentImageInViewer = null;
        StateHasChanged();
    }

    private async Task NavigateToRespondToReport()
    {
        if (_report != null)
        {
            Navigation.NavigateTo($"/respond-to-report/{_report.Id}");
        }

        await Task.CompletedTask;
    }

    private async Task MarkReportAsResolvedAsync()
    {
        if (_report == null)
        {
            return;
        }

        _isLoading = true;
        StateHasChanged();

        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.PatchAsync($"{DevUISettings.ApiUrl}/Report/mark-as-resolved/{_report.Id}", null)
        );

        if (response.IsSuccessStatusCode)
        {
            ToastService.ShowSuccess("Report marked as resolved successfully.");
            Navigation.NavigateTo("/all-reports");
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

            string errorMsg = await HandleResponse(errorResponse);
            await HandleInvalidResponse($"Failed to mark report as resolved. {errorMsg}");
        }

        _isLoading = false;
        StateHasChanged();
    }
}