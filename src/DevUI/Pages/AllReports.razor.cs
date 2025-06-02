using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Blazored.Toast.Services;
using DevUI.Common.Interfaces;
using DevUI.Interfaces;
using Domain.Common;
using Shared.Dtos;
using Shared.Payload.Responses;

namespace DevUI.Pages;

public partial class AllReports : ComponentBase, IPageComponent
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDevUISettings DevUISettings { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;

    private readonly List<ReportDto> _reports = [];
    private int _currentPage = 1;
    private const int PageSize = 15;
    private bool _isLoading;

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
        await LoadReports();
    }

    private async Task LoadReports()
    {
        _isLoading = true;

        HttpResponseMessage response = await HttpClientService.SendRequestAsync(() =>
            Http.GetAsync($"{DevUISettings.ApiUrl}/Report/get-all?page={_currentPage}&pageSize={PageSize}")
        );

        if (response.IsSuccessStatusCode)
        {
            GetAllReportsResponse? getAllReportsResponse =
                await response.Content.ReadFromJsonAsync<GetAllReportsResponse>();
            if (getAllReportsResponse?.Reports is not null)
            {
                _reports.AddRange(getAllReportsResponse.Reports);
                _currentPage++;
            }
            else
            {
                string errorMessage = await HandleResponse(getAllReportsResponse);
                await HandleInvalidResponse(errorMessage);
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

            string errorMessage = await HandleResponse(errorResponse);
            await HandleInvalidResponse(errorMessage);
        }

        _isLoading = false;
    }

    private async Task LoadMoreReports()
    {
        await LoadReports();
    }

    private void NavigateToReportDetails(Guid reportId)
    {
        Navigation.NavigateTo($"/report-details/{reportId}");
    }

    private string GetStatusClass(string status)
    {
        return status.ToLower() switch
        {
            "pending" => "status-pending",
            "inprogress" => "status-inprogress",
            "resolved" => "status-resolved",
            _ => "status-unknown"
        };
    }
}