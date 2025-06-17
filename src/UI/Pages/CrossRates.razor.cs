using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text;
using Blazored.Toast.Services;
using Domain.Common;
using Domain.Constants;
using Shared.Dtos;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Rate;
using UI.Common.Interfaces;
using UI.Services;
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages
{
    public partial class CrossRates : ComponentBase, IAsyncDisposable, IPageComponent
    {
        [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
        [Inject] private HttpClient Http { get; set; } = null!;
        [Inject] private IJSRuntime Js { get; set; } = null!;
        [Inject] private IToastService ToastService { get; set; } = null!;
        [Inject] private IConfiguration Configuration { get; set; } = null!;
        [Inject] private LocalizationService Localizer { get; set; } = null!;
        [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        private DateTime _endDate = DateTime.Today;
        private string _startDateString = string.Empty;
        private string _endDateString = string.Empty;

        private List<string> _currencies = [];
        private string? _selectedCurrency1;
        private string? _selectedCurrency2;

        private decimal[] _chartData = [];
        private string[] _dates = [];
        private string _chartLabel = string.Empty;

        private bool _isLoadingData;
        private bool _showTable;
        private bool _dataLoaded;
        
        private string _pageTitle = "";
        private string _headerTitle = "";
        private string _headerDescription = "";
        private string _labelCurrency1 = "";
        private string _labelCurrency2 = "";
        private string _labelStartDate = "";
        private string _labelEndDate = "";
        private string _buttonFetchRates = "";
        private string _buttonShowChart = "";
        private string _buttonShowTable = "";
        private string _loadingData = "";
        private string _tableHeaderDate = "";
        private string _tableHeaderRate = "";
        private string _tableNoData = "";
        private string _buttonDownloadPdf = "";
        private string _errorGeneric = "";
        private string _errorTryAgain = "";
        private string _errorMessagePrefix = "";
        private string _errorStatusCodePrefix = "";
        private string _errorErrorsPrefix = "";
        private string _errorSelectCurrencies = "";
        private string _errorLoadingCurrencies = "";
        private string _errorFetchingCrossRates = "";
        private string _initialPrompt = "";

        protected override async Task OnInitializedAsync()
        {
            await LoadLocalizedStringsAsync();
            UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
            SetStringDates();
            await LoadCurrencyListAsync();
            if (_currencies.Count >= 1)
            {
                _selectedCurrency1 = _currencies[0];
                if (_currencies.Count >= 2)
                {
                     _selectedCurrency2 = _currencies.FirstOrDefault(c => c != _selectedCurrency1) ?? _currencies[1];
                }
            }
        }
        
        private async Task HandleSettingsChangedAsync()
        {
            await LoadLocalizedStringsAsync();
            StateHasChanged();
        }

        private async Task LoadLocalizedStringsAsync()
        {
            _pageTitle = await Localizer.GetStringAsync("cross_rates.page_title");
            _headerTitle = await Localizer.GetStringAsync("cross_rates.header_title");
            _headerDescription = await Localizer.GetStringAsync("cross_rates.header_description");
            _labelCurrency1 = await Localizer.GetStringAsync("cross_rates.label.currency1");
            _labelCurrency2 = await Localizer.GetStringAsync("cross_rates.label.currency2");
            _labelStartDate = await Localizer.GetStringAsync("cross_rates.label.start_date");
            _labelEndDate = await Localizer.GetStringAsync("cross_rates.label.end_date");
            _buttonFetchRates = await Localizer.GetStringAsync("cross_rates.button.fetch_rates");
            _buttonShowChart = await Localizer.GetStringAsync("cross_rates.button.show_chart");
            _buttonShowTable = await Localizer.GetStringAsync("cross_rates.button.show_table");
            _loadingData = await Localizer.GetStringAsync("cross_rates.loading_data");
            _tableHeaderDate = await Localizer.GetStringAsync("cross_rates.table.header_date");
            _tableHeaderRate = await Localizer.GetStringAsync("cross_rates.table.header_rate");
            _tableNoData = await Localizer.GetStringAsync("cross_rates.table.no_data");
            _buttonDownloadPdf = await Localizer.GetStringAsync("cross_rates.button.download_pdf");
            
            _errorGeneric = await Localizer.GetStringAsync("settings.error.generic_processing");
            _errorTryAgain = await Localizer.GetStringAsync("settings.toast.default_error_try_again");
            _errorMessagePrefix = await Localizer.GetStringAsync("settings.error.message_prefix");
            _errorStatusCodePrefix = await Localizer.GetStringAsync("settings.error.status_code_prefix");
            _errorErrorsPrefix = await Localizer.GetStringAsync("settings.error.errors_prefix");

            _errorSelectCurrencies = await Localizer.GetStringAsync("cross_rates.error.select_currencies");
            _errorLoadingCurrencies = await Localizer.GetStringAsync("cross_rates.error.loading_currencies");
            _errorFetchingCrossRates = await Localizer.GetStringAsync("cross_rates.error.fetching_cross_rates");
            _initialPrompt = await Localizer.GetStringAsync("cross_rates.initial_prompt");
        }

        private void SetStringDates()
        {
            _startDateString = _startDate.ToString(DateConstants.DateFormat);
            _endDateString = _endDate.ToString(DateConstants.DateFormat);
        }
        
        private async Task OnDateChangedAsync()
        {
            SetStringDates();
            _dataLoaded = false; 
            await InvokeAsync(StateHasChanged);
        }
        
        private async Task OnCurrencyChangedAsync()
        {
            _dataLoaded = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task LoadCurrencyListAsync()
        {
            _isLoadingData = true;
            StateHasChanged();
            string tempStartDate = DateTime.Today.AddYears(-1).ToString(DateConstants.DateFormat);
            string tempEndDate = DateTime.Today.ToString(DateConstants.DateFormat);
            string url = $"{Configuration.ApiUrl}/Rate/get-all-currencies-in-range?StartDateString={tempStartDate}&EndDateString={tempEndDate}";
            try
            {
                HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
                GetAllCurrenciesResponse? apiResponse = await resp.Content.ReadFromJsonAsync<GetAllCurrenciesResponse>();

                if (resp.IsSuccessStatusCode && apiResponse?.Currencies != null)
                {
                    _currencies = apiResponse.Currencies
                        .Where(c => !string.IsNullOrWhiteSpace(c.Code))
                        .Select(c => c.Code.Trim().ToUpperInvariant())
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                }
                else
                {
                    string err = await HandleResponse(apiResponse);
                    await HandleInvalidResponse(err);
                }
            }
            catch (Exception ex)
            {
                await HandleInvalidResponse(string.Format(_errorLoadingCurrencies, ex.Message));
            }
            finally
            {
                _isLoadingData = false;
                StateHasChanged();
            }
        }

        private async Task FetchCrossRatesAsync()
        {
            if (string.IsNullOrEmpty(_selectedCurrency1) || string.IsNullOrEmpty(_selectedCurrency2))
            {
                ToastService.ShowError(await Localizer.GetStringAsync("cross_rates.error.select_both_currencies"));
                return;
            }
            if (_selectedCurrency1 == _selectedCurrency2)
            {
                ToastService.ShowError(_errorSelectCurrencies);
                return;
            }

            _isLoadingData = true;
            _dataLoaded = false;
            StateHasChanged();

            SetStringDates(); 

            CrossRateRequest request = new CrossRateRequest
            {
                Currency = _selectedCurrency1,
                Currency2 = _selectedCurrency2,
                StartDateString = _startDateString,
                EndDateString = _endDateString
            };
            
            string url = $"{Configuration.ApiUrl}/Rate/get-cross-rates?Currency={request.Currency}&Currency2={request.Currency2}&StartDateString={request.StartDateString}&EndDateString={request.EndDateString}";

            try
            {
                HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
                GetCrossRatesResponse? response = await resp.Content.ReadFromJsonAsync<GetCrossRatesResponse>();

                if (resp.IsSuccessStatusCode && response is { Success: true, CrossRates: not null })
                {
                    List<CrossRateDto> rates = response.CrossRates.OrderBy(r => r.Date).ToList();
                    _chartData = rates.Select(r => r.Rate).ToArray();
                    _dates = rates.Select(r => r.Date.ToString("yyyy-MM-dd")).ToArray();
                    _chartLabel = $"{_selectedCurrency1}/{_selectedCurrency2}";
                    _dataLoaded = true;
                    if (_chartData.Length == 0)
                    {
                        ToastService.ShowInfo(_tableNoData);
                    }
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
                _chartData = [];
                _dates = [];
                await HandleInvalidResponse(string.Format(_errorFetchingCrossRates, ex.Message));
            }
            finally
            {
                _isLoadingData = false;
                StateHasChanged();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (_dataLoaded && !_isLoadingData && !_showTable && _chartData.Length > 0 && _dates.Length > 0)
            {
                await Js.InvokeVoidAsync("drawChart", _chartData, _dates);
            }
        }

        private void ToggleTable()
        {
            _showTable = !_showTable;
            StateHasChanged();
        }

        private async Task DownloadTablePdf()
        {
            if (_dataLoaded && _chartData.Length != 0)
            {
                await Js.InvokeVoidAsync("downloadTableAsPdf", "crossRatesTable", $"CrossRates_{_selectedCurrency1}-{_selectedCurrency2}_{_startDateString}_to_{_endDateString}.pdf");
            }
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
        
        public async ValueTask DisposeAsync()
        {
            UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
            await Task.CompletedTask;
            GC.SuppressFinalize(this);
        }
    }
}