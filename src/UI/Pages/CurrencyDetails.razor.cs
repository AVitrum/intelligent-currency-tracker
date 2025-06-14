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
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class CurrencyDetails : ComponentBase, IPageComponent, IAsyncDisposable
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
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private string _pageTitle = "";
    private string _loadingMessage = "";
    private string _noDataTitle = "";
    private string _noDataDescription = "";
    private string _noDataButtonBackToDashboard = "";
    private string _analyticsHeaderFormat = "";
    private string _sectionStatisticalOverview = "";
    private string _statMeanValue = "";
    private string _statMedianValue = "";
    private string _statMaxValue = "";
    private string _statMinValue = "";
    private string _statStdDeviation = "";
    private string _statVariance = "";
    private string _statDataPoints = "";
    private string _statDateRange = "";
    private string _sectionRateTrend = "";
    private string _chartNoData = "";
    private string _chartFooterTextFormat = "";
    private string _buttonDownloadPdf = "";
    private string _buttonBackToDashboard = "";

    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorExceptionPrefix = "";
    private string _toastChartDisplayError = "";
    private string _toastLoadDataErrorFormat = "";
    private string _toastLoadRateTrendErrorFormat = "";
    private string _toastPdfNoData = "";
    private string _toastPdfPreparing = "";
    private string _toastPdfGenerationErrorFormat = "";


    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;

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
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("currencydetails.page_title_format");
        _loadingMessage = await Localizer.GetStringAsync("currencydetails.loading_message");
        _noDataTitle = await Localizer.GetStringAsync("currencydetails.nodata.title");
        _noDataDescription = await Localizer.GetStringAsync("currencydetails.nodata.description");
        _noDataButtonBackToDashboard =
            await Localizer.GetStringAsync("currencydetails.nodata.button.back_to_dashboard");
        _analyticsHeaderFormat = await Localizer.GetStringAsync("currencydetails.analytics_header_format");
        _sectionStatisticalOverview = await Localizer.GetStringAsync("currencydetails.section.statistical_overview");
        _statMeanValue = await Localizer.GetStringAsync("currencydetails.stats.mean_value");
        _statMedianValue = await Localizer.GetStringAsync("currencydetails.stats.median_value");
        _statMaxValue = await Localizer.GetStringAsync("currencydetails.stats.max_value");
        _statMinValue = await Localizer.GetStringAsync("currencydetails.stats.min_value");
        _statStdDeviation = await Localizer.GetStringAsync("currencydetails.stats.std_deviation");
        _statVariance = await Localizer.GetStringAsync("currencydetails.stats.variance");
        _statDataPoints = await Localizer.GetStringAsync("currencydetails.stats.data_points");
        _statDateRange = await Localizer.GetStringAsync("currencydetails.stats.date_range");
        _sectionRateTrend = await Localizer.GetStringAsync("currencydetails.section.rate_trend");
        _chartNoData = await Localizer.GetStringAsync("currencydetails.chart.no_data");
        _chartFooterTextFormat = await Localizer.GetStringAsync("currencydetails.chart.footer_format");
        _buttonDownloadPdf = await Localizer.GetStringAsync("currencydetails.button.download_pdf");
        _buttonBackToDashboard = await Localizer.GetStringAsync("currencydetails.button.back_to_dashboard");

        _errorGeneric = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
        _errorExceptionPrefix = await Localizer.GetStringAsync("settings.error.exception_prefix");
        _toastChartDisplayError = await Localizer.GetStringAsync("currencydetails.toast.chart_display_error");
        _toastLoadDataErrorFormat = await Localizer.GetStringAsync("currencydetails.toast.load_data_error_format");
        _toastLoadRateTrendErrorFormat =
            await Localizer.GetStringAsync("currencydetails.toast.load_rate_trend_error_format");
        _toastPdfNoData = await Localizer.GetStringAsync("currencydetails.toast.pdf.no_data");
        _toastPdfPreparing = await Localizer.GetStringAsync("currencydetails.toast.pdf.preparing");
        _toastPdfGenerationErrorFormat =
            await Localizer.GetStringAsync("currencydetails.toast.pdf.generation_error_format");

        if (_currencyDetails?.CurrencyPair != null)
        {
            _headerTitle = string.Format(_analyticsHeaderFormat, _currencyDetails.CurrencyPair);
        }
        else if (!string.IsNullOrEmpty(CurrencyCode))
        {
            _headerTitle = string.Format(_analyticsHeaderFormat, CurrencyCode);
        }
        else
        {
            _headerTitle = "";
        }
    }

    private string _headerTitle = "";


    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    public Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return Task.FromResult(_errorGeneric);
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
            catch (Exception)
            {
                await HandleInvalidResponse(_toastChartDisplayError);
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
            if (_currencyDetails?.CurrencyPair != null) 
            {
                _headerTitle = string.Format(_analyticsHeaderFormat, _currencyDetails.CurrencyPair);
            }
            else if (!string.IsNullOrEmpty(CurrencyCode))
            {
                _headerTitle = string.Format(_analyticsHeaderFormat, CurrencyCode);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_toastLoadDataErrorFormat, ex.Message));
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
        else if (analyticsResp.StatusCode != HttpStatusCode.NotFound)
        {
            GetDetailsResponse? errorResponse = await analyticsResp.Content.ReadFromJsonAsync<GetDetailsResponse>();
            string err = await HandleResponse(errorResponse);
            await HandleInvalidResponse(err);
        }

        _currencyDetails ??= null;
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
            await HandleInvalidResponse(string.Format(_toastLoadRateTrendErrorFormat, CurrencyCode));
        }
    }

    private void NavigateToDashboard()
    {
        NavigationManager.NavigateTo("/dashboard");
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}