using System.Net;
using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Domain.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Helpers;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Rate;
using Shared.Payload.Responses.UserRate;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;


namespace UI.Pages;

public partial class Dashboard : ComponentBase, IPageComponent, IAsyncDisposable
{
    private DateTime _startDate = DateTime.Today.AddMonths(-1);
    private DateTime _endDate = DateTime.Today;

    private string _startDateString = string.Empty;
    private string _endDateString = string.Empty;

    private readonly List<string> _currencies = [];
    private readonly List<string> _pinnedCurrencies = [];

    private readonly Dictionary<string, (decimal[] Data, string[] Dates)> _pinnedData =
        new Dictionary<string, (decimal[] Data, string[] Dates)>();

    private decimal[] _chartData = [];
    private string[] _dates = [];
    private string? _lastPinnedCurrency;

    private string _selectedChartType = "line";
    private string _selectedCurrency = "USD";
    private bool _shouldDrawNewPin;
    private bool _showTable;
    private bool _isLoadingChartData;

    [Inject] private WebSocketService WebSocketService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private string _pageTitle = "";
    private string _headerTitle = "";
    private string _headerDescription = "";
    private string _labelCurrency = "";
    private string _labelStartDate = "";
    private string _labelEndDate = "";
    private string _labelChartType = "";
    private string _buttonPinCurrency = "";
    private string _buttonViewDetails = "";
    private string _buttonShowChart = "";
    private string _buttonShowTable = "";
    private string _loadingChartData = "";
    private string _tableHeaderDate = "";
    private string _tableHeaderRate = "";
    private string _tableNoData = "";
    private string _buttonDownloadPdf = "";
    private string _noDataAvailable = "";
    private string _chartFooterText = "";
    private string _pinnedChartsTitle = "";
    private string _buttonRemovePinned = "";
    private string _chartTypeLine = "";
    private string _chartTypeCandlestick = "";

    private string _errorGeneric = "";
    private string _errorTryAgain = "";
    private string _toastNoCurrencyForRange = "";
    private string _toastSelectCurrencyToPin = "";
    private string _toastCurrencyAlreadyPinned = "";
    private string _toastCurrencyPinnedSuccessfully = "";
    private string _toastSelectCurrencyFirst = "";
    private string _errorLoadingTrackedCurrencies = "";
    private string _errorRequestingPinnedData = "";
    private string _errorLoadingCurrencyList = "";
    private string _errorWebsocketLoadingRates = "";
    private string _errorPinningCurrency = "";
    private string _errorRemovingPinnedCurrency = "";
    private string _errorMessagePrefix = "";
    private string _errorStatusCodePrefix = "";
    private string _errorErrorsPrefix = "";
    private string _errorExceptionPrefix = "";


    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitle = await Localizer.GetStringAsync("dashboard.page_title");
        _headerTitle = await Localizer.GetStringAsync("dashboard.header_title");
        _headerDescription = await Localizer.GetStringAsync("dashboard.header_description");
        _labelCurrency = await Localizer.GetStringAsync("dashboard.label.currency");
        _labelStartDate = await Localizer.GetStringAsync("dashboard.label.start_date");
        _labelEndDate = await Localizer.GetStringAsync("dashboard.label.end_date");
        _labelChartType = await Localizer.GetStringAsync("dashboard.label.chart_type");
        _buttonPinCurrency = await Localizer.GetStringAsync("dashboard.button.pin_currency");
        _buttonViewDetails = await Localizer.GetStringAsync("dashboard.button.view_details");
        _buttonShowChart = await Localizer.GetStringAsync("dashboard.button.show_chart");
        _buttonShowTable = await Localizer.GetStringAsync("dashboard.button.show_table");
        _loadingChartData = await Localizer.GetStringAsync("dashboard.loading_chart_data");
        _tableHeaderDate = await Localizer.GetStringAsync("dashboard.table.header_date");
        _tableHeaderRate = await Localizer.GetStringAsync("dashboard.table.header_rate");
        _tableNoData = await Localizer.GetStringAsync("dashboard.no_data_available");
        _buttonDownloadPdf = await Localizer.GetStringAsync("dashboard.button.download_pdf");
        _noDataAvailable = await Localizer.GetStringAsync("dashboard.no_data_available");
        _chartFooterText = await Localizer.GetStringAsync("dashboard.chart_footer_text");
        _pinnedChartsTitle = await Localizer.GetStringAsync("dashboard.pinned_charts_title");
        _buttonRemovePinned = await Localizer.GetStringAsync("dashboard.button.remove_pinned");
        _chartTypeLine = await Localizer.GetStringAsync("dashboard.chart_type.line");
        _chartTypeCandlestick = await Localizer.GetStringAsync("dashboard.chart_type.candlestick");

        _errorGeneric = await Localizer.GetStringAsync("settings.error.generic_processing");
        _errorTryAgain = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _toastNoCurrencyForRange = await Localizer.GetStringAsync("dashboard.toast.no_currency_selected_for_range");
        _toastSelectCurrencyToPin = await Localizer.GetStringAsync("dashboard.toast.select_currency_to_pin");
        _toastCurrencyAlreadyPinned = await Localizer.GetStringAsync("dashboard.toast.currency_already_pinned");
        _toastCurrencyPinnedSuccessfully =
            await Localizer.GetStringAsync("dashboard.toast.currency_pinned_successfully");
        _toastSelectCurrencyFirst = await Localizer.GetStringAsync("dashboard.toast.select_currency_first");
        _errorLoadingTrackedCurrencies = await Localizer.GetStringAsync("dashboard.error.loading_tracked_currencies");
        _errorRequestingPinnedData = await Localizer.GetStringAsync("dashboard.error.requesting_pinned_data");
        _errorLoadingCurrencyList = await Localizer.GetStringAsync("dashboard.error.loading_currency_list");
        _errorWebsocketLoadingRates = await Localizer.GetStringAsync("dashboard.error.websocket_loading_rates");
        _errorPinningCurrency = await Localizer.GetStringAsync("dashboard.error.pinning_currency");
        _errorRemovingPinnedCurrency = await Localizer.GetStringAsync("dashboard.error.removing_pinned_currency");
        _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");
        _errorExceptionPrefix = await Localizer.GetStringAsync("settings.error.exception_prefix");
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    public async Task<string> HandleResponse(BaseResponse? response)
    {
        if (response is null)
        {
            return _errorGeneric;
        }

        StringBuilder errorMessage = new StringBuilder();
        errorMessage.AppendLine($"{_errorMessagePrefix} {response.Message}");
        errorMessage.AppendLine($"{_errorStatusCodePrefix} {response.StatusCode}");
        if (response.Errors.Any())
        {
            errorMessage.AppendLine($"{_errorErrorsPrefix} {string.Join(", ", response.Errors)}");
        }

        return await Task.FromResult(errorMessage.ToString());
    }

    public async Task HandleInvalidResponse(string? message = null)
    {
        ToastService.ShowError(message ?? _errorTryAgain);
        await Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;

        _isLoadingChartData = true;
        SetStringDates();
        await WebSocketService.ConnectAsync();
        await LoadTrackedCurrenciesAsync();

        await LoadCurrencyListAsync();
        await FetchMainChartRatesAsync();
        await RequestDataForPinnedCurrenciesAsync();

        _isLoadingChartData = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldDrawNewPin && _lastPinnedCurrency is not null &&
            _pinnedData.TryGetValue(_lastPinnedCurrency, out (decimal[] Data, string[] Dates) chartInfo))
        {
            await Js.InvokeVoidAsync(
                "drawMiniChart",
                $"miniChart-{_lastPinnedCurrency}",
                $"miniTooltip-{_lastPinnedCurrency}",
                chartInfo.Data,
                chartInfo.Dates
            );
            _shouldDrawNewPin = false;
        }

        if (!_isLoadingChartData && !_showTable && _chartData is { Length: > 0 } && _dates is { Length: > 0 })
        {
            if (_selectedChartType == "candlestick")
            {
                await Js.InvokeVoidAsync("drawCandlestickChart", _chartData, _dates);
            }
            else
            {
                await Js.InvokeVoidAsync("drawChart", _chartData, _dates);
            }
        }
    }

    private void SetStringDates()
    {
        _startDateString = _startDate.ToString(DateConstants.DateFormat);
        _endDateString = _endDate.ToString(DateConstants.DateFormat);
    }

    private async Task LoadTrackedCurrenciesAsync()
    {
        string url = $"{Configuration.ApiUrl}/userRate/tracked-currencies";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetTrackedCurrenciesResponse? response =
                await resp.Content.ReadFromJsonAsync<GetTrackedCurrenciesResponse>();
            if (resp.IsSuccessStatusCode && response?.Data != null)
            {
                _pinnedCurrencies.Clear();
                _pinnedCurrencies.AddRange(response.Data.Select(c => c.Code.Trim().ToUpperInvariant()));
            }
            else if (resp.StatusCode != HttpStatusCode.NotFound)
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_errorLoadingTrackedCurrencies, ex.Message));
        }
    }

    private async Task RequestDataForPinnedCurrenciesAsync()
    {
        foreach (string currency in _pinnedCurrencies)
        {
            string url =
                $"{Configuration.ApiUrl}/Rate/get-range?StartDateString={_startDateString}&EndDateString={_endDateString}&Currency={currency}";
            try
            {
                HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
                GetRatesResponse? response = await resp.Content.ReadFromJsonAsync<GetRatesResponse>();
                if (resp.IsSuccessStatusCode && response?.Rates != null)
                {
                    List<RateDto> rates = (List<RateDto>)response.Rates;
                    if (rates.Count != 0)
                    {
                        decimal[] data = rates.Select(r => r.Value).ToArray();
                        string[] dates = rates.Select(r => r.Date).ToArray();
                        _pinnedData[currency] = (data, dates);
                    }
                    else
                    {
                        _pinnedData.Remove(currency);
                    }
                }
                else
                {
                    _pinnedData.Remove(currency);
                }
            }
            catch (Exception ex)
            {
                _pinnedData.Remove(currency);
                await HandleInvalidResponse(string.Format(_errorRequestingPinnedData, currency, ex.Message));
            }
        }
    }

    private async Task LoadCurrencyListAsync()
    {
        string previouslySelectedCurrencyNormalized = string.IsNullOrEmpty(_selectedCurrency)
            ? string.Empty
            : _selectedCurrency.Trim().ToUpperInvariant();

        _currencies.Clear();

        string url =
            $"{Configuration.ApiUrl}/Rate/get-all-currencies-in-range?StartDateString={_startDateString}&EndDateString={_endDateString}";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetAllCurrenciesResponse? apiResponse = await resp.Content.ReadFromJsonAsync<GetAllCurrenciesResponse>();

            if (resp.IsSuccessStatusCode && apiResponse?.Currencies != null)
            {
                List<string> normalizedNewCurrencies = apiResponse.Currencies
                    .Where(c => !string.IsNullOrWhiteSpace(c.Code))
                    .Select(c => c.Code.Trim().ToUpperInvariant())
                    .Distinct()
                    .ToList();
                normalizedNewCurrencies.Sort();

                _currencies.AddRange(normalizedNewCurrencies);

                if (!string.IsNullOrEmpty(previouslySelectedCurrencyNormalized) &&
                    _currencies.Contains(previouslySelectedCurrencyNormalized))
                {
                    _selectedCurrency = _currencies.First(c => c == previouslySelectedCurrencyNormalized);
                }
                else if (_currencies.Count != 0)
                {
                    _selectedCurrency = _currencies.First();
                }
                else
                {
                    _selectedCurrency = string.Empty;
                }
            }
            else
            {
                _selectedCurrency = string.Empty;
                string err = await HandleResponse(apiResponse);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            _selectedCurrency = string.Empty;
            await HandleInvalidResponse(string.Format(_errorLoadingCurrencyList, ex.Message));
        }
    }

    private async Task FetchMainChartRatesAsync()
    {
        if (string.IsNullOrEmpty(_selectedCurrency))
        {
            _chartData = [];
            _dates = [];
            ToastService.ShowInfo(_toastNoCurrencyForRange);
            return;
        }

        string start = _startDate.ToString(DateHelper.GetDateFormat());
        string end = _endDate.ToString(DateHelper.GetDateFormat());
        try
        {
            ExchangeRateRequest request = new ExchangeRateRequest
            {
                StartDateString = start,
                EndDateString = end,
                Currency = _selectedCurrency
            };
            GetRatesResponse? response = await WebSocketService.RequestRatesAsync(request);
            if (response is { Success: true, Rates: not null })
            {
                List<RateDto> rates = (List<RateDto>)response.Rates;
                _chartData = rates.Select(r => r.Value).ToArray();
                _dates = rates.Select(r => r.Date).ToArray();
            }
            else
            {
                _chartData = [];
                _dates = [];
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_errorWebsocketLoadingRates, ex.Message));
            _chartData = [];
            _dates = [];
        }
    }

    private async Task OnDateChangedAsync()
    {
        _isLoadingChartData = true;
        StateHasChanged();

        SetStringDates();

        string previousSelectedCurrencyValue = _selectedCurrency;
        await LoadCurrencyListAsync();

        bool currencySelectionChangedProgrammatically = previousSelectedCurrencyValue != _selectedCurrency;

        StateHasChanged();

        if (currencySelectionChangedProgrammatically)
        {
            await RequestDataForPinnedCurrenciesAsync();
            return;
        }

        await FetchMainChartRatesAsync();
        await RequestDataForPinnedCurrenciesAsync();
        _isLoadingChartData = false;
        StateHasChanged();
    }

    private async Task OnCurrencyChangedAsync()
    {
        _isLoadingChartData = true;
        StateHasChanged();

        await FetchMainChartRatesAsync();

        _isLoadingChartData = false;
        StateHasChanged();
    }

    private async Task OnChartTypeChangedAsync()
    {
        await InvokeAsync(StateHasChanged);
    }

    private async Task PinCurrentCurrency()
    {
        if (string.IsNullOrEmpty(_selectedCurrency))
        {
            ToastService.ShowInfo(_toastSelectCurrencyToPin);
            return;
        }

        string normalizedSelectedCurrency = _selectedCurrency.Trim().ToUpperInvariant();
        if (_pinnedCurrencies.Contains(normalizedSelectedCurrency))
        {
            ToastService.ShowInfo(string.Format(_toastCurrencyAlreadyPinned, _selectedCurrency));
            return;
        }

        _isLoadingChartData = true;
        StateHasChanged();

        string url = $"{Configuration.ApiUrl}/userRate/track-currency";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() =>
                Http.PostAsJsonAsync(url, new TrackCurrencyRequest { Currency = _selectedCurrency }));
            TrackCurrencyResponse? response = await resp.Content.ReadFromJsonAsync<TrackCurrencyResponse>();
            if (resp.IsSuccessStatusCode && response is { Success: true })
            {
                ToastService.ShowSuccess(string.Format(_toastCurrencyPinnedSuccessfully, _selectedCurrency));
                if (!_pinnedCurrencies.Contains(normalizedSelectedCurrency))
                {
                    _pinnedCurrencies.Add(normalizedSelectedCurrency);
                }

                string rangedUrl =
                    $"{Configuration.ApiUrl}/Rate/get-range?StartDateString={_startDateString}&EndDateString={_endDateString}&Currency={_selectedCurrency}";
                HttpResponseMessage rateResp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(rangedUrl));
                GetRatesResponse? rateDataResponse = await rateResp.Content.ReadFromJsonAsync<GetRatesResponse>();
                if (rateResp.IsSuccessStatusCode && rateDataResponse?.Rates != null)
                {
                    List<RateDto> rates = (List<RateDto>)rateDataResponse.Rates;
                    if (rates.Count != 0)
                    {
                        _pinnedData[normalizedSelectedCurrency] = (rates.Select(r => r.Value).ToArray(),
                            rates.Select(r => r.Date).ToArray());
                    }
                    else
                    {
                        _pinnedData.Remove(normalizedSelectedCurrency);
                    }
                }

                _lastPinnedCurrency = _selectedCurrency;
                _shouldDrawNewPin = true;
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_errorPinningCurrency, ex.Message));
        }
        finally
        {
            _isLoadingChartData = false;
            StateHasChanged();
        }
    }

    private async Task RemovePinnedCurrency(string code)
    {
        _isLoadingChartData = true;
        StateHasChanged();

        string normalizedCode = code.Trim().ToUpperInvariant();
        string url = $"{Configuration.ApiUrl}/userRate/remove-tracked-currency";
        try
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = JsonContent.Create(new RemoveTrackedCurrencyRequest(code))
            };
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.SendAsync(request));
            RemoveTrackedCurrencyResponse? response =
                await resp.Content.ReadFromJsonAsync<RemoveTrackedCurrencyResponse>();
            if (resp.IsSuccessStatusCode && response is { Success: true })
            {
                ToastService.ShowSuccess(response.Message); // Assuming response.Message is localized or a key
                _pinnedCurrencies.Remove(normalizedCode);
                _pinnedData.Remove(normalizedCode);
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse(string.Format(_errorRemovingPinnedCurrency, ex.Message));
        }
        finally
        {
            _isLoadingChartData = false;
            StateHasChanged();
        }
    }

    private void ToggleTable()
    {
        _showTable = !_showTable;
        StateHasChanged();
    }

    private async Task DownloadTablePdf()
    {
        await Js.InvokeVoidAsync("downloadTableAsPdf", "ratesTable");
    }

    private void NavigateToDetails()
    {
        if (string.IsNullOrEmpty(_selectedCurrency))
        {
            ToastService.ShowError(_toastSelectCurrencyFirst);
            return;
        }

        string navStartDateString = _startDate.ToString(DateConstants.DateFormat);
        string navEndDateString = _endDate.ToString(DateConstants.DateFormat);
        NavigationManager.NavigateTo($"/currency/{_selectedCurrency}/{navStartDateString}/{navEndDateString}");
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}