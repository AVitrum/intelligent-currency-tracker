using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using DevUI.Common.Interfaces;
using DevUI.Interfaces;
using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Requests;
using Shared.Payload.Responses;
using Shared.Payload.Responses.Report;

namespace DevUI.Pages;

public partial class RespondToReport : ComponentBase, IPageComponent
{
    [Parameter] public Guid ReportId { get; set; }

    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;

    private ReportDto? _report;
    private bool _isLoading;
    private bool _isSubmitting;
    private string _errorMessage = string.Empty;
    private string _responseMessage = string.Empty;
    private string _validationError = string.Empty;

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

    private async Task HandleRespondToReportAsync()
    {
        _validationError = string.Empty;
        if (string.IsNullOrWhiteSpace(_responseMessage))
        {
            _validationError = "Response message cannot be empty.";
            StateHasChanged();
            return;
        }

        if (_report == null)
        {
            ToastService.ShowError("Report details not loaded. Cannot submit response.");
            return;
        }

        _isSubmitting = true;
        StateHasChanged();

        RespondRequest request = new RespondRequest { Message = _responseMessage };

        JsonContent content = JsonContent.Create(request);

        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.PostAsync($"{DevUISettings.ApiUrl}/Report/respond/{_report.Id}", content)
        );

        if (response.IsSuccessStatusCode)
        {
            ToastService.ShowSuccess("Response submitted successfully.");
            Navigation.NavigateTo($"/report-details/{_report.Id}");
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
            await HandleInvalidResponse($"Failed to submit response. {errorMsg}");
        }

        _isSubmitting = false;
        StateHasChanged();
    }

    private async Task NavigateBack()
    {
        if (_report != null)
        {
            Navigation.NavigateTo($"/report-details/{_report.Id}");
        }
        else
        {
            Navigation.NavigateTo("/all-reports");
        }

        await Task.CompletedTask;
    }

    private async Task NavigateToReportList()
    {
        Navigation.NavigateTo("/all-reports");
        await Task.CompletedTask;
    }
}