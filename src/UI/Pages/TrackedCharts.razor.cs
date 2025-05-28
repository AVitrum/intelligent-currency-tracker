using System.Net;
using System.Net.Http.Json;
using System.Text;
using Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Shared.Dtos;
using Shared.Helpers;
using Shared.Payload.Requests;
using Shared.Payload.Responses.Rate;
using Shared.Payload.Responses.UserRate;
using UI.Common.Interfaces;

namespace UI.Pages;

public partial class TrackedCharts : ComponentBase, IPageComponent
{
    private readonly DateTime _startDate = DateTime.Today.AddMonths(-1);
    
    private readonly DateTime _endDate = DateTime.Today;

    private readonly List<string> _pinnedCurrencies = [];
    
    private readonly Dictionary<string, (decimal[] Data, string[] Dates)> _pinnedData =
        new Dictionary<string, (decimal[] Data, string[] Dates)>();

    private string _startDateString = string.Empty;
    
    private string _endDateString = string.Empty;

    private string? _lastPinnedCurrency;

    private bool _shouldDrawNewPin;
    
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
        SetDateStrings();
        await LoadPinnedCurrenciesAsync();
        await RequestDataForPinnedCurrenciesAsync();
        await DrawPinnedChartsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldDrawNewPin && _lastPinnedCurrency is not null)
        {
            (decimal[] data, string[] dates) = _pinnedData[_lastPinnedCurrency];

            await JS.InvokeVoidAsync(
                "drawMiniChart",
                $"miniChart-{_lastPinnedCurrency}",
                $"miniTooltip-{_lastPinnedCurrency}",
                data,
                dates
            );

            _shouldDrawNewPin = false;
        }
    }

    private void SetDateStrings()
    {
        _startDateString = _startDate.ToString(DateHelper.GetDateFormat());
        _endDateString = _endDate.ToString(DateHelper.GetDateFormat());
    }

    private async Task LoadPinnedCurrenciesAsync()
    {
        string url = $"{Configuration.ApiUrl}/userRate/tracked-currencies";

        try
        {
            HttpResponseMessage resp = await HttpClientService.SendRequestAsync(() => Http.GetAsync(url));
            GetTrackedCurrenciesResponse? response =
                await resp.Content.ReadFromJsonAsync<GetTrackedCurrenciesResponse>();

            if (resp.IsSuccessStatusCode)
            {
                List<CurrencyDto> list = (List<CurrencyDto>)response!.Data!;

                if (list.Count > 0)
                {
                    _pinnedCurrencies.AddRange(list.Select(c => c.Code));
                }
            }
            else if (resp.StatusCode == HttpStatusCode.NotFound) { }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error: {ex.Message}");
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


                if (resp.IsSuccessStatusCode)
                {
                    List<RateDto> rates = (List<RateDto>)response!.Rates!;

                    if (rates.Count > 0)
                    {
                        decimal[] data = rates.Select(r => r.Value).ToArray();
                        string[] dates = rates.Select(r => r.Date).ToArray();

                        _pinnedData[currency] = (data, dates);
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleInvalidResponse($"Error: {ex.Message}");
            }
        }
    }

    private async Task DrawPinnedChartsAsync()
    {
        foreach (string currency in _pinnedCurrencies)
        {
            _lastPinnedCurrency = currency;
            _shouldDrawNewPin = true;
            StateHasChanged();
        }

        await Task.CompletedTask;
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

            if (resp.IsSuccessStatusCode)
            {
                ToastService.ShowSuccess(response!.Message);
                _pinnedCurrencies.Remove(_pinnedCurrencies.Find(c => c == code)!);
                await RequestDataForPinnedCurrenciesAsync();
                await DrawPinnedChartsAsync();
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error: {ex.Message}");
        }
    }
}