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

public partial class CurrencyChart : ComponentBase, IPageComponent
{
    private readonly List<string> _currencies = [];
    private readonly List<string> _pinnedCurrencies = [];

    private readonly Dictionary<string, (decimal[] Data, string[] Dates)> _pinnedData =
        new Dictionary<string, (decimal[] Data, string[] Dates)>();

    private decimal[] _chartData = [];
    private string[] _dates = [];
    private DateTime _endDate = DateTime.Today;
    private string? _lastPinnedCurrency;

    private string _selectedChartType = "line";
    private string _selectedCurrency = "USD";
    private bool _shouldDrawNewPin;
    private bool _showTable;
    private DateTime _startDate = DateTime.Today.AddMonths(-1);

    [Inject] private WebSocketService WebSocketService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IToastService ToastService { get; set; } = null!;
    [Inject] private IConfiguration Configuration { get; set; } = null!;
    [Inject] private IHttpClientService HttpClientService { get; set; } = null!;
    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;


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
        await WebSocketService.ConnectAsync();
        await LoadTrackedCurrenciesAsync();
        await RequestDataForPinnedCurrenciesAsync();
        await DrawPinnedChartsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadCurrencyListAsync();
        await LoadRatesAsync();
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

        if (!_showTable && _chartData is { Length: > 0 } && _dates is { Length: > 0 })
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

    private async Task DrawPinnedChartsAsync()
    {
        foreach (string currency in _pinnedCurrencies.Where(currency => _pinnedData.ContainsKey(currency)))
        {
            _lastPinnedCurrency = currency;
            _shouldDrawNewPin = true;
            StateHasChanged();
        }

        await Task.CompletedTask;
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
                _pinnedCurrencies.AddRange(response.Data.Select(c => c.Code));
            }
            else if (resp.StatusCode != HttpStatusCode.NotFound)
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error loading tracked currencies: {ex.Message}");
        }
    }

    private async Task RequestDataForPinnedCurrenciesAsync()
    {
        foreach (string currency in _pinnedCurrencies)
        {
            string start = _startDate.ToString(DateHelper.GetDateFormat());
            string end = _endDate.ToString(DateHelper.GetDateFormat());
            string url =
                $"{Configuration.ApiUrl}/Rate/get-range?StartDateString={start}&EndDateString={end}&Currency={currency}";
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
                }
            }
            catch (Exception ex)
            {
                await HandleInvalidResponse($"Error requesting pinned data for {currency}: {ex.Message}");
            }
        }
    }

    private async Task LoadCurrencyListAsync()
    {
        if (_currencies.Count != 0)
        {
            return;
        }

        string url = $"{Configuration.ApiUrl}/Rate/get-all-currencies";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetAllCurrenciesResponse? response = await resp.Content.ReadFromJsonAsync<GetAllCurrenciesResponse>();
            if (resp.IsSuccessStatusCode && response?.Currencies != null)
            {
                _currencies.AddRange(response.Currencies.Select(c => c.Code));
                _currencies.Sort();
                if (_currencies.Count != 0 && string.IsNullOrEmpty(_selectedCurrency))
                {
                    _selectedCurrency = _currencies.First();
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
            await HandleInvalidResponse($"Error loading currency list: {ex.Message}");
        }
    }

    private async Task LoadRatesAsync()
    {
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

            StateHasChanged();
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"WebSocket error loading rates: {ex.Message}");
        }
    }

    private async Task PinCurrentCurrency()
    {
        if (string.IsNullOrEmpty(_selectedCurrency) || _pinnedCurrencies.Contains(_selectedCurrency))
        {
            ToastService.ShowInfo($"Currency '{_selectedCurrency}' is already pinned or invalid.");
            return;
        }

        string url = $"{Configuration.ApiUrl}/userRate/track-currency";
        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() =>
                Http.PostAsJsonAsync(url, new TrackCurrencyRequest { Currency = _selectedCurrency }));
            TrackCurrencyResponse? response = await resp.Content.ReadFromJsonAsync<TrackCurrencyResponse>();
            if (resp.IsSuccessStatusCode && response is { Success: true })
            {
                ToastService.ShowSuccess($"Currency '{_selectedCurrency}' pinned successfully.");
                _pinnedCurrencies.Add(_selectedCurrency);
                await RequestDataForPinnedCurrenciesAsync();
                _lastPinnedCurrency = _selectedCurrency;
                _shouldDrawNewPin = true;
                StateHasChanged();
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error pinning currency: {ex.Message}");
        }
    }

    private async Task RemovePinnedCurrency(string code)
    {
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
                ToastService.ShowSuccess(response.Message);
                _pinnedCurrencies.Remove(code);
                _pinnedData.Remove(code);
                StateHasChanged();
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error removing pinned currency: {ex.Message}");
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
            ToastService.ShowError("Please select a currency first.");
            return;
        }

        string startDateString = _startDate.ToString(DateConstants.DateFormat);
        string endDateString = _endDate.ToString(DateConstants.DateFormat);
        NavigationManager.NavigateTo($"/currency/{_selectedCurrency}/{startDateString}/{endDateString}");
    }
}