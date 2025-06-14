using System.Net;
using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Domain.Constants;
using Microsoft.AspNetCore.Components;
using Shared.Dtos;
using Shared.Payload.Responses.Rate;
using UI.Common.Interfaces;
using UI.Services; 
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class CurrencyComparison : ComponentBase, IPageComponent, IAsyncDisposable
{
    private DateTime _startDate = DateTime.Today.AddMonths(-1);
    private DateTime _endDate = DateTime.Today;

    private ComparativeAnalyticsDto? _comparisonDetails;
    private List<string> _currencies = [];
    private bool _isLoading;
    private string? _selectedCurrency1;
    private string? _selectedCurrency2;

    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private string _pageTitle = "";
    private string _headerTitle = "";
    private string _labelBaseCurrency = "";
    private string _labelCompareWith = "";
    private string _labelStartDate = "";
    private string _labelEndDate = "";
    private string _optionLoadingCurrencies = "";
    private string _buttonCompare = "";
    private string _buttonComparing = "";
    private string _loadingMessage = "";
    private string _summaryTitle = "";
    private string _summaryAnalysisPeriod = "";
    private string _summaryCorrelationCoefficient = "";
    private string _summaryAverageSpread = "";
    private string _summarySpreadStdDev = "";
    private string _detailsHeaderFormat = "";
    private string _noDataInstruction = "";
    private string _buttonDashboard = "";

    private string _statsMeanValue = "";
    private string _statsMedianValue = "";
    private string _statsMaxValue = "";
    private string _statsMinValue = "";
    private string _statsValueRange = "";
    private string _statsStdDeviation = "";
    private string _statsVariance = "";
    private string _statsMovingAveragesTitle = "";
    private string _statsMovingAverageFormat = "";
    private string _statsNoMovingAverageData = "";

    private string _toastSelectTwoCurrencies = "";
    private string _toastSelectDifferentCurrencies = "";
    private string _toastLoadSuccess = "";
    private string _toastNoDataFound = "";
    private string _errorLoadingCurrencyListFormat = "";
    private string _errorFetchingComparisonDataFormat = "";

    private string _errorGeneric = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
        await LoadCurrencyListAsync();
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("currencycomparison.page_title");
        _headerTitle = await Localizer.GetStringAsync("currencycomparison.header_title");
        _labelBaseCurrency = await Localizer.GetStringAsync("currencycomparison.label.base_currency");
        _labelCompareWith = await Localizer.GetStringAsync("currencycomparison.label.compare_with");
        _labelStartDate = await Localizer.GetStringAsync("currencycomparison.label.start_date");
        _labelEndDate = await Localizer.GetStringAsync("currencycomparison.label.end_date");
        _optionLoadingCurrencies = await Localizer.GetStringAsync("currencycomparison.option.loading_currencies");
        _buttonCompare = await Localizer.GetStringAsync("currencycomparison.button.compare");
        _buttonComparing = await Localizer.GetStringAsync("currencycomparison.button.comparing");
        _loadingMessage = await Localizer.GetStringAsync("currencycomparison.loading_message");
        _summaryTitle = await Localizer.GetStringAsync("currencycomparison.summary.title");
        _summaryAnalysisPeriod = await Localizer.GetStringAsync("currencycomparison.summary.analysis_period");
        _summaryCorrelationCoefficient = await Localizer.GetStringAsync("currencycomparison.summary.correlation_coefficient");
        _summaryAverageSpread = await Localizer.GetStringAsync("currencycomparison.summary.average_spread");
        _summarySpreadStdDev = await Localizer.GetStringAsync("currencycomparison.summary.spread_std_dev");
        _detailsHeaderFormat = await Localizer.GetStringAsync("currencycomparison.details.header_format");
        _noDataInstruction = await Localizer.GetStringAsync("currencycomparison.nodata_instruction");
        _buttonDashboard = await Localizer.GetStringAsync("currencycomparison.button.dashboard");

        _statsMeanValue = await Localizer.GetStringAsync("currencydetails.stats.mean_value");
        _statsMedianValue = await Localizer.GetStringAsync("currencydetails.stats.median_value");
        _statsMaxValue = await Localizer.GetStringAsync("currencydetails.stats.max_value");
        _statsMinValue = await Localizer.GetStringAsync("currencydetails.stats.min_value");
        _statsValueRange = await Localizer.GetStringAsync("currencycomparison.stats.value_range");
        _statsStdDeviation = await Localizer.GetStringAsync("currencydetails.stats.std_deviation");
        _statsVariance = await Localizer.GetStringAsync("currencydetails.stats.variance");
        _statsMovingAveragesTitle = await Localizer.GetStringAsync("currencycomparison.stats.moving_averages_title");
        _statsMovingAverageFormat = await Localizer.GetStringAsync("currencycomparison.stats.moving_average_format");
        _statsNoMovingAverageData = await Localizer.GetStringAsync("currencycomparison.stats.no_moving_average_data");

        _toastSelectTwoCurrencies = await Localizer.GetStringAsync("currencycomparison.toast.select_two_currencies");
        _toastSelectDifferentCurrencies = await Localizer.GetStringAsync("currencycomparison.toast.select_different_currencies");
        _toastLoadSuccess = await Localizer.GetStringAsync("currencycomparison.toast.load_success");
        _toastNoDataFound = await Localizer.GetStringAsync("currencycomparison.toast.no_data_found");
        _errorLoadingCurrencyListFormat = await Localizer.GetStringAsync("currencycomparison.error.loading_currency_list_format");
        _errorFetchingComparisonDataFormat = await Localizer.GetStringAsync("currencycomparison.error.fetching_comparison_data_format");

        _errorGeneric = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
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

    private async Task LoadCurrencyListAsync()
    {
        _isLoading = true;
        string url = $"{Configuration.ApiUrl}/Rate/get-all-currencies";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetAllCurrenciesResponse? response = await resp.Content.ReadFromJsonAsync<GetAllCurrenciesResponse>();

            if (resp.IsSuccessStatusCode && response?.Currencies != null)
            {
                _currencies = response.Currencies.Select(c => c.Code).ToList();
                _currencies.Sort();
                if (_currencies.Count > 1)
                {
                    _selectedCurrency1 = _currencies[0];
                    _selectedCurrency2 = _currencies[1];
                }
                else if (_currencies.Count == 1)
                {
                    _selectedCurrency1 = _currencies[0];
                }
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_errorLoadingCurrencyListFormat, ex.Message));
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task HandleCompareCurrenciesAsync()
    {
        if (string.IsNullOrEmpty(_selectedCurrency1) || string.IsNullOrEmpty(_selectedCurrency2))
        {
            ToastService.ShowWarning(_toastSelectTwoCurrencies);
            return;
        }

        if (_selectedCurrency1 == _selectedCurrency2)
        {
            ToastService.ShowWarning(_toastSelectDifferentCurrencies);
            return;
        }

        _isLoading = true;
        _comparisonDetails = null;
        StateHasChanged();

        string startDateString = _startDate.ToString(DateConstants.DateFormat);
        string endDateString = _endDate.ToString(DateConstants.DateFormat);

        string url = $"{Configuration.ApiUrl}/Rate/compare-currencies?" +
                     $"CurrencyCode1={_selectedCurrency1}&" +
                     $"CurrencyCode2={_selectedCurrency2}&" +
                     $"StartDateString={startDateString}&" +
                     $"EndDateString={endDateString}";

        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            CompareCurrenciesResponse? response = await resp.Content.ReadFromJsonAsync<CompareCurrenciesResponse>();

            if (resp.IsSuccessStatusCode && response?.ComparativeAnalytics != null)
            {
                _comparisonDetails = response.ComparativeAnalytics;
                ToastService.ShowSuccess(_toastLoadSuccess);
            }
            else if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _comparisonDetails = null;
                ToastService.ShowInfo(_toastNoDataFound);
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_errorFetchingComparisonDataFormat, ex.Message));
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void NavigateToDashboard()
    {
        NavigationManager.NavigateTo("/dashboard");
    }

    private RenderFragment RenderSingleCurrencyAnalytics(SingleCurrencyAnalyticsDto details)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "stats-grid");

            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "stat-item");
            builder.OpenElement(4, "span");
            builder.AddAttribute(5, "class", "stat-label");
            builder.AddContent(6, _statsMeanValue);
            builder.CloseElement();
            builder.OpenElement(7, "span");
            builder.AddAttribute(8, "class", "stat-value");
            builder.AddContent(9, details.MeanValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "stat-item");
            builder.OpenElement(12, "span");
            builder.AddAttribute(13, "class", "stat-label");
            builder.AddContent(14, _statsMedianValue);
            builder.CloseElement();
            builder.OpenElement(15, "span");
            builder.AddAttribute(16, "class", "stat-value");
            builder.AddContent(17, details.MedianValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(18, "div");
            builder.AddAttribute(19, "class", "stat-item");
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "class", "stat-label");
            builder.AddContent(22, _statsMaxValue);
            builder.CloseElement();
            builder.OpenElement(23, "span");
            builder.AddAttribute(24, "class", "stat-value");
            builder.AddContent(25, details.MaxValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(26, "div");
            builder.AddAttribute(27, "class", "stat-item");
            builder.OpenElement(28, "span");
            builder.AddAttribute(29, "class", "stat-label");
            builder.AddContent(30, _statsMinValue);
            builder.CloseElement();
            builder.OpenElement(31, "span");
            builder.AddAttribute(32, "class", "stat-value");
            builder.AddContent(33, details.MinValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(34, "div");
            builder.AddAttribute(35, "class", "stat-item");
            builder.OpenElement(36, "span");
            builder.AddAttribute(37, "class", "stat-label");
            builder.AddContent(38, _statsValueRange);
            builder.CloseElement();
            builder.OpenElement(39, "span");
            builder.AddAttribute(40, "class", "stat-value");
            builder.AddContent(41, details.Range.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(42, "div");
            builder.AddAttribute(43, "class", "stat-item");
            builder.OpenElement(44, "span");
            builder.AddAttribute(45, "class", "stat-label");
            builder.AddContent(46, _statsStdDeviation);
            builder.CloseElement();
            builder.OpenElement(47, "span");
            builder.AddAttribute(48, "class", "stat-value");
            builder.AddContent(49, details.StandardDeviation.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(50, "div");
            builder.AddAttribute(51, "class", "stat-item");
            builder.OpenElement(52, "span");
            builder.AddAttribute(53, "class", "stat-label");
            builder.AddContent(54, _statsVariance);
            builder.CloseElement();
            builder.OpenElement(55, "span");
            builder.AddAttribute(56, "class", "stat-value");
            builder.AddContent(57, details.Variance.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.CloseElement();

            if (details.MovingAverages.Any())
            {
                builder.OpenElement(58, "h3");
                builder.AddAttribute(59, "class", "sub-section-title");
                builder.AddContent(60, _statsMovingAveragesTitle);
                builder.CloseElement();

                builder.OpenElement(61, "div");
                builder.AddAttribute(62, "class", "moving-averages-grid");
                foreach (MovingAverageDto ma in details.MovingAverages)
                {
                    builder.OpenElement(63, "div");
                    builder.AddAttribute(64, "class", "ma-item");
                    builder.OpenElement(65, "span");
                    builder.AddAttribute(66, "class", "ma-label");
                    builder.AddContent(67, string.Format(_statsMovingAverageFormat, ma.Period));
                    builder.CloseElement();
                    builder.OpenElement(68, "span");
                    builder.AddAttribute(69, "class", "ma-value");
                    builder.AddContent(70, ma.Value.ToString("N4"));
                    builder.CloseElement();
                    builder.CloseElement(); // ma-item
                }
            }
            else
            {
                builder.OpenElement(71, "p");
                builder.AddAttribute(72, "class", "no-ma-data");
                builder.AddContent(73, _statsNoMovingAverageData);
            }

            builder.CloseElement();
        };
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}