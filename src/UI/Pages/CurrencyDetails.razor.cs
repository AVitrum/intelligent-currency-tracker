using System.Net;
using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Payload.Responses.Rate;
using UI.Common.Interfaces;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class CurrencyDetails : ComponentBase, IPageComponent
{
    private bool _chartDataReadyForRender;

    private decimal[]? _chartRateData;
    private string[]? _chartRateDates;

    private SingleCurrencyAnalyticsDto? _currencyDetails;
    private bool _isLoading = true;
    [Parameter] public required string CurrencyCode { get; set; }
    [Parameter] public required string StartDate { get; set; }
    [Parameter] public required string EndDate { get; set; }

    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;

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
        _isLoading = true;
        _chartDataReadyForRender = false;
        if (string.IsNullOrEmpty(CurrencyCode))
        {
            NavigationManager.NavigateTo("/");
            _isLoading = false;
            return;
        }

        await LoadDataAsync();
        _isLoading = false;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_isLoading && _chartDataReadyForRender && _chartRateData != null && _chartRateDates != null &&
            _chartRateData.Length != 0)
        {
            try
            {
                await Js.InvokeVoidAsync(
                    "drawMiniChart",
                    $"currencyDetailChart-{CurrencyCode}",
                    $"currencyDetailTooltip-{CurrencyCode}",
                    _chartRateData,
                    _chartRateDates
                );
                _chartDataReadyForRender = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error drawing chart: {ex.Message}");
                await HandleInvalidResponse("Could not display the currency trend chart.");
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadDataAsync()
    {
        _currencyDetails = null;
        _chartRateData = null;
        _chartRateDates = null;
        _chartDataReadyForRender = false;

        try
        {
            await LoadAnalyticsDataAsync();
            await LoadChartDataAsync();
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error loading currency page data: {ex.Message}");
        }
    }

    private async Task LoadAnalyticsDataAsync()
    {
        string analyticsUrl =
            $"{Configuration.ApiUrl}/rate/get-details?StartDateString={StartDate}&EndDateString={EndDate}&Currency={CurrencyCode}";

        HttpResponseMessage analyticsResp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(analyticsUrl));
        if (analyticsResp.IsSuccessStatusCode)
        {
            GetDetailsResponse? analyticsResponse = await analyticsResp.Content.ReadFromJsonAsync<GetDetailsResponse>();
            _currencyDetails = analyticsResponse?.Details;
        }
        else if (analyticsResp.StatusCode == HttpStatusCode.NotFound)
        {
            _currencyDetails = null;
        }
        else
        {
            _currencyDetails = null;
            GetDetailsResponse? errorResponse = await analyticsResp.Content.ReadFromJsonAsync<GetDetailsResponse>();
            string err = await HandleResponse(errorResponse);
            await HandleInvalidResponse(err);
        }
    }

    private async Task LoadChartDataAsync()
    {
        if (string.IsNullOrEmpty(CurrencyCode))
        {
            return;
        }

        string ratesUrl =
            $"{Configuration.ApiUrl}/Rate/get-range?StartDateString={StartDate}&EndDateString={EndDate}&Currency={CurrencyCode}";

        HttpResponseMessage ratesResp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(ratesUrl));
        if (ratesResp.IsSuccessStatusCode)
        {
            GetRatesResponse? ratesResponse = await ratesResp.Content.ReadFromJsonAsync<GetRatesResponse>();
            if (ratesResponse?.Rates != null && ratesResponse.Rates.Any())
            {
                List<RateDto> rates = (List<RateDto>)ratesResponse.Rates;
                _chartRateData = rates.Select(r => r.Value).ToArray();
                _chartRateDates = rates.Select(r => r.Date).ToArray();
                _chartDataReadyForRender = true;
            }
        }
        else
        {
            Console.WriteLine($"Failed to load chart data for {CurrencyCode}. Status: {ratesResp.StatusCode}");
            await HandleInvalidResponse($"Could not load rate trend data for {CurrencyCode}.");
        }
    }

    private void NavigateToDashboard()
    {
        NavigationManager.NavigateTo("/dashboard");
    }
}