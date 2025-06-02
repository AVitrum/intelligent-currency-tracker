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
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class Forecast : ComponentBase, IPageComponent
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

    public Task HandleInvalidResponse(string message = "An error occurred. Please try again.")
    {
        ToastService.ShowError(message);
        return Task.CompletedTask;
    }

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await LoadCurrencyListAsync();
        if (!string.IsNullOrEmpty(SelectedCurrencyCode) && ForecastPeriods > 0)
        {
            await HandleForecastAsync();
        }

        _isLoading = false;
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
            await HandleInvalidResponse($"Error loading currency list: {ex.Message}");
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
                ToastService.ShowError("Please select a currency.");
            }

            _forecastData = [];
            _forecastDates = [];
            _isLoading = false;
            StateHasChanged();
            return;
        }

        if (_forecastPeriodsField <= 0)
        {
            ToastService.ShowError("Forecast periods must be greater than zero.");
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
            ToastService.ShowError("Selected currency details not found.");
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
                ToastService.ShowSuccess("Forecast generated successfully.");
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
            await HandleInvalidResponse($"Error fetching forecast: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }
}