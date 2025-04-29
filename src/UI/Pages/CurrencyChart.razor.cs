using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Helpers;

namespace UI.Pages;

public partial class CurrencyChart : ComponentBase, IPageComponent
{
    private readonly List<string> _currencies = [];
    private string _selectedCurrency = "USD";
    private DateTime _startDate = DateTime.Today.AddMonths(-1);
    private DateTime _endDate = DateTime.Today;

    private decimal[] _chartData = null!;
    private string[] _dates = null!;

    protected override async Task OnParametersSetAsync()
    {
        await LoadCurrencyListAsync();
        await LoadRatesAsync();
    }
    
    private async Task LoadCurrencyListAsync()
    {
        string url = $"{UISettings.ApiUrl}/Rate/get-all-currencies";
        try
        {
            HttpResponseMessage response = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));

            if (response.IsSuccessStatusCode)
            {
                ICollection<CurrencyDto>? currencies = await response.Content.ReadFromJsonAsync<ICollection<CurrencyDto>>();
                if (currencies == null || currencies.Count == 0)
                {
                    await HandleInvalidResponse("No currencies available.");
                    return;
                }

                _currencies.AddRange(currencies.Select(c => c.Code).ToList());
                _currencies.Sort();
            }
            else
            {
                string errorResponse = await HandleResponse(response);
                await HandleInvalidResponse(errorResponse);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error: {ex.Message}");
        }
    }


    private async Task LoadRatesAsync()
    {
        string start = _startDate.ToString(DateHelper.GetDateFormat());
        string end = _endDate.ToString(DateHelper.GetDateFormat());
        string url = $"{UISettings.ApiUrl}/Rate/get-range?StartDateString={start}&EndDateString={end}&Currency={_selectedCurrency}";

        try
        {
            HttpResponseMessage response = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));

            if (response.IsSuccessStatusCode)
            {
                ICollection<RateDto>? rates = await response.Content.ReadFromJsonAsync<ICollection<RateDto>>();
                if (rates == null || rates.Count == 0)
                {
                    await HandleInvalidResponse("No data available for the selected date range.");
                    return;
                }

                _chartData = rates.Select(r => r.Value).ToArray();
                _dates = rates.Select(r => r.Date).ToArray();

                await JS.InvokeVoidAsync("drawChart", _chartData, _dates);
            }
            else
            {
                string errorResponse = await HandleResponse(response);
                await HandleInvalidResponse(errorResponse);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error: {ex.Message}");
        }
    }

    public Task HandleInvalidResponse(string message = "An error occurred while processing your request. Try again later.")
    {
        ToastService.ShowError(message);
        return Task.CompletedTask;
    }

    public async Task<string> HandleResponse(HttpResponseMessage response)
    {
        string errorResponse = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(errorResponse))
        {
            return "Something went wrong. Please try again.";
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(errorResponse);
            if (doc.RootElement.TryGetProperty("title", out JsonElement titleElement))
            {
                return titleElement.GetString() ?? errorResponse;
            }

            if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement))
            {
                return messageElement.GetString() ?? errorResponse;
            }

            return errorResponse;
        }
        catch (JsonException)
        {
            return errorResponse;
        }
    }
}