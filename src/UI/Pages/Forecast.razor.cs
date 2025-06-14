using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Payload.Requests;
using Shared.Payload.Responses.AiModel;
using Shared.Payload.Responses.Rate;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Forecast : ComponentBase, IPageComponent, IAsyncDisposable
{
    private readonly List<CurrencyDto> _currencyDtos = [];
    private readonly List<string> _currencyCodes = [];
    private string _selectedCurrencyCodeField = string.Empty;
    private int _forecastPeriodsField = 7;

    private double[] _forecastData = [];
    private string[] _forecastDates = [];
    private bool _isLoading = true;
    private bool _forecastFetched;

    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    private string _pageTitleString = "";
    private string _headerTitleString = "";
    private string _headerDescriptionString = "";
    private string _labelSelectCurrencyString = "";
    private string _loadingCurrenciesString = "";
    private string _noCurrenciesAvailableString = "";
    private string _labelForecastPeriodString = "";
    private string _generatingForecastString = "";
    private string _disclaimerStrongString = "";
    private string _disclaimerTextString = "";
    private string _loadingForecastDataString = "";
    private string _noForecastDataString = "";
    private string _chartFooterTextString = "";
    private string _tableHeaderDateString = "";
    private string _tableHeaderValueString = "";

    private string _toastSelectCurrencyString = "";
    private string _toastInvalidPeriodString = "";
    private string _toastCurrencyDetailsNotFoundString = "";
    private string _toastForecastSuccessString = "";
    private string _errorLoadingCurrencyListFormatString = "";
    private string _errorFetchingForecastFormatString = "";
    
    private string _errorGenericString = "";
    private string _errorMessagePrefixString = "";
    private string _errorStatusCodePrefixString = "";
    private string _errorErrorsPrefixString = "";
    
    private string SelectedCurrencyCode
    {
        get => _selectedCurrencyCodeField;
        set
        {
            if (_selectedCurrencyCodeField != value)
            {
                _selectedCurrencyCodeField = value;
                _ = OnCurrencyOrPeriodChangedAsync();
            }
        }
    }

    private int ForecastPeriods
    {
        get => _forecastPeriodsField;
        set
        {
            if (_forecastPeriodsField != value)
            {
                _forecastPeriodsField = value;
                _ = OnCurrencyOrPeriodChangedAsync();
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
        await LoadCurrencyListAsync();
        if (!string.IsNullOrEmpty(SelectedCurrencyCode) && ForecastPeriods > 0)
        {
            await HandleForecastAsync();
        }
        _isLoading = false;
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _pageTitleString = await Localizer.GetStringAsync("forecast.page_title");
        _headerTitleString = await Localizer.GetStringAsync("forecast.header.title");
        _headerDescriptionString = await Localizer.GetStringAsync("forecast.header.description");
        _labelSelectCurrencyString = await Localizer.GetStringAsync("forecast.form.label_currency");
        _loadingCurrenciesString = await Localizer.GetStringAsync("forecast.form.loading_currencies");
        _noCurrenciesAvailableString = await Localizer.GetStringAsync("forecast.form.no_currencies");
        _labelForecastPeriodString = await Localizer.GetStringAsync("forecast.form.label_period");
        _generatingForecastString = await Localizer.GetStringAsync("forecast.form.generating_forecast");
        _disclaimerStrongString = await Localizer.GetStringAsync("forecast.disclaimer.strong");
        _disclaimerTextString = await Localizer.GetStringAsync("forecast.disclaimer.text");
        _loadingForecastDataString = await Localizer.GetStringAsync("forecast.chart.loading_data");
        _noForecastDataString = await Localizer.GetStringAsync("forecast.chart.no_data");
        _chartFooterTextString = await Localizer.GetStringAsync("forecast.chart.footer");
        _tableHeaderDateString = await Localizer.GetStringAsync("forecast.table.header_date");
        _tableHeaderValueString = await Localizer.GetStringAsync("forecast.table.header_value");

        _toastSelectCurrencyString = await Localizer.GetStringAsync("forecast.toast.select_currency");
        _toastInvalidPeriodString = await Localizer.GetStringAsync("forecast.toast.invalid_period");
        _toastCurrencyDetailsNotFoundString = await Localizer.GetStringAsync("forecast.toast.currency_details_not_found");
        _toastForecastSuccessString = await Localizer.GetStringAsync("forecast.toast.forecast_success");
        _errorLoadingCurrencyListFormatString = await Localizer.GetStringAsync("forecast.error.loading_currency_list_format");
        _errorFetchingForecastFormatString = await Localizer.GetStringAsync("forecast.error.fetching_forecast_format");

        _errorGenericString = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
        _errorMessagePrefixString = await Localizer.GetStringAsync("settings.error.message_prefix");
        _errorStatusCodePrefixString = await Localizer.GetStringAsync("settings.error.status_code_prefix");
        _errorErrorsPrefixString = await Localizer.GetStringAsync("settings.error.errors_prefix");
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
            return Task.FromResult(_errorGenericString);
        }

        StringBuilder errorMessage = new StringBuilder();
        errorMessage.AppendLine($"{_errorMessagePrefixString} {response.Message}");
        errorMessage.AppendLine($"{_errorStatusCodePrefixString} {response.StatusCode}");
        if (response.Errors.Any())
        {
            errorMessage.AppendLine($"{_errorErrorsPrefixString} {string.Join(", ", response.Errors)}");
        }

        return Task.FromResult(errorMessage.ToString());
    }

    public Task HandleInvalidResponse(string? message = null)
    {
        ToastService.ShowError(message ?? _errorGenericString);
        return Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_forecastFetched && _forecastData.Length > 0 && _forecastDates.Length > 0)
        {
            await Js.InvokeVoidAsync("drawChart", _forecastData, _forecastDates);
            _forecastFetched = false;
        }
    }

    private async Task LoadCurrencyListAsync()
    {
        string previouslySelectedCurrencyNormalized = string.IsNullOrEmpty(_selectedCurrencyCodeField)
            ? string.Empty
            : _selectedCurrencyCodeField.Trim().ToUpperInvariant();

        _currencyDtos.Clear();
        _currencyCodes.Clear();

        string url = $"{Configuration.ApiUrl}/Rate/get-all-currencies";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetAllCurrenciesResponse? apiResponse = await resp.Content.ReadFromJsonAsync<GetAllCurrenciesResponse>();

            if (resp.IsSuccessStatusCode && apiResponse?.Currencies != null)
            {
                _currencyDtos.AddRange(apiResponse.Currencies
                    .Where(c => !string.IsNullOrWhiteSpace(c.Code))
                    .DistinctBy(c => c.Code.Trim().ToUpperInvariant())
                    .OrderBy(c => c.Code));

                _currencyCodes.AddRange(_currencyDtos.Select(c => c.Code.Trim().ToUpperInvariant()));

                if (!string.IsNullOrEmpty(previouslySelectedCurrencyNormalized) &&
                    _currencyCodes.Contains(previouslySelectedCurrencyNormalized))
                {
                    _selectedCurrencyCodeField = _currencyCodes.First(c => c == previouslySelectedCurrencyNormalized);
                }
                else if (_currencyCodes.Count > 0)
                {
                    _selectedCurrencyCodeField = _currencyCodes.First();
                }
                else
                {
                    _selectedCurrencyCodeField = string.Empty;
                }
            }
            else
            {
                _selectedCurrencyCodeField = string.Empty;
                string err = await HandleResponse(apiResponse);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            _selectedCurrencyCodeField = string.Empty;
            await HandleInvalidResponse(string.Format(_errorLoadingCurrencyListFormatString, ex.Message));
        }
    }

    private async Task OnCurrencyOrPeriodChangedAsync()
    {
        _forecastData = [];
        _forecastDates = [];
        await InvokeAsync(StateHasChanged);
        await HandleForecastAsync();
    }

    private async Task HandleForecastAsync()
    {
        if (string.IsNullOrEmpty(_selectedCurrencyCodeField))
        {
            if (_currencyCodes.Count != 0)
            {
                ToastService.ShowError(_toastSelectCurrencyString);
            }
            _forecastData = [];
            _forecastDates = [];
            _isLoading = false;
            StateHasChanged();
            return;
        }

        if (_forecastPeriodsField <= 0)
        {
            ToastService.ShowError(_toastInvalidPeriodString);
            _forecastData = [];
            _forecastDates = [];
            _isLoading = false;
            StateHasChanged();
            return;
        }

        CurrencyDto? selectedDto = _currencyDtos.FirstOrDefault(c =>
            c.Code.Trim().ToUpperInvariant() == _selectedCurrencyCodeField.Trim().ToUpperInvariant());
        if (selectedDto == null)
        {
            ToastService.ShowError(_toastCurrencyDetailsNotFoundString);
            _forecastData = [];
            _forecastDates = [];
            _isLoading = false;
            StateHasChanged();
            return;
        }

        _isLoading = true;
        StateHasChanged();

        ForecastRequest request = new ForecastRequest(selectedDto.R030, _forecastPeriodsField);
        string url = $"{Configuration.ApiUrl}/AiModel/forecast";

        try
        {
            HttpResponseMessage resp =
                await HttpClientService.SendRequestAsync(() => Http.PostAsJsonAsync(url, request));
            ForecastResponse? response = await resp.Content.ReadFromJsonAsync<ForecastResponse>();

            if (resp.IsSuccessStatusCode && response is { Success: true, Forecast: not null })
            {
                _forecastData = response.Forecast.Select(p => p.Yhat).ToArray();
                _forecastDates = response.Forecast.Select(p => p.Ds).ToArray();
                _forecastFetched = true;
                ToastService.ShowSuccess(_toastForecastSuccessString);
            }
            else
            {
                _forecastData = [];
                _forecastDates = [];
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            _forecastData = [];
            _forecastDates = [];
            await HandleInvalidResponse(string.Format(_errorFetchingForecastFormatString, ex.Message));
        }
        finally
        {
            _isLoading = false;
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