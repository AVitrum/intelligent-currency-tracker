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
using IConfiguration = UI.Common.Interfaces.IConfiguration;

namespace UI.Pages;

public partial class CurrencyComparison : ComponentBase, IPageComponent
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
        await LoadCurrencyListAsync();
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
            await HandleInvalidResponse($"Error loading currency list: {ex.Message}");
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
            ToastService.ShowWarning("Please select two currencies to compare.");
            return;
        }

        if (_selectedCurrency1 == _selectedCurrency2)
        {
            ToastService.ShowWarning("Please select two different currencies.");
            return;
        }

        _isLoading = true;
        _comparisonDetails = null; // Clear previous results
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
                ToastService.ShowSuccess("Currency comparison data loaded successfully.");
            }
            else if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _comparisonDetails = null;
                ToastService.ShowInfo("No comparison data found for the selected criteria.");
            }
            else
            {
                string err = await HandleResponse(response);
                await HandleInvalidResponse(err);
            }
        }
        catch (Exception ex)
        {
            await HandleInvalidResponse($"Error fetching comparison data: {ex.Message}");
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

    private static RenderFragment RenderSingleCurrencyAnalytics(SingleCurrencyAnalyticsDto details)
    {
        return builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "stats-grid");

            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "stat-item");
            builder.AddMarkupContent(4, "<span class=\"stat-label\">Mean Value:</span> ");
            builder.OpenElement(5, "span");
            builder.AddAttribute(6, "class", "stat-value");
            builder.AddContent(7, details.MeanValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(8, "div");
            builder.AddAttribute(9, "class", "stat-item");
            builder.AddMarkupContent(10, "<span class=\"stat-label\">Median Value:</span> ");
            builder.OpenElement(11, "span");
            builder.AddAttribute(12, "class", "stat-value");
            builder.AddContent(13, details.MedianValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(14, "div");
            builder.AddAttribute(15, "class", "stat-item");
            builder.AddMarkupContent(16, "<span class=\"stat-label\">Maximum Value:</span> ");
            builder.OpenElement(17, "span");
            builder.AddAttribute(18, "class", "stat-value");
            builder.AddContent(19, details.MaxValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "stat-item");
            builder.AddMarkupContent(22, "<span class=\"stat-label\">Minimum Value:</span> ");
            builder.OpenElement(23, "span");
            builder.AddAttribute(24, "class", "stat-value");
            builder.AddContent(25, details.MinValue.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(26, "div");
            builder.AddAttribute(27, "class", "stat-item");
            builder.AddMarkupContent(28, "<span class=\"stat-label\">Value Range:</span> ");
            builder.OpenElement(29, "span");
            builder.AddAttribute(30, "class", "stat-value");
            builder.AddContent(31, details.Range.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(32, "div");
            builder.AddAttribute(33, "class", "stat-item");
            builder.AddMarkupContent(34, "<span class=\"stat-label\">Standard Deviation:</span> ");
            builder.OpenElement(35, "span");
            builder.AddAttribute(36, "class", "stat-value");
            builder.AddContent(37, details.StandardDeviation.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.OpenElement(38, "div");
            builder.AddAttribute(39, "class", "stat-item");
            builder.AddMarkupContent(40, "<span class=\"stat-label\">Variance:</span> ");
            builder.OpenElement(41, "span");
            builder.AddAttribute(42, "class", "stat-value");
            builder.AddContent(43, details.Variance.ToString("N4"));
            builder.CloseElement();
            builder.CloseElement();

            builder.CloseElement();

            if (details.MovingAverages.Any())
            {
                builder.OpenElement(44, "h3");
                builder.AddAttribute(45, "class", "sub-section-title");
                builder.AddContent(46, "Moving Averages");
                builder.CloseElement();

                builder.OpenElement(47, "div");
                builder.AddAttribute(48, "class", "moving-averages-grid");
                foreach (MovingAverageDto ma in details.MovingAverages)
                {
                    builder.OpenElement(49, "div");
                    builder.AddAttribute(50, "class", "ma-item");
                    builder.OpenElement(51, "span");
                    builder.AddAttribute(52, "class", "ma-label");
                    builder.AddContent(53, $"{ma.Period}-Day MA:");
                    builder.CloseElement();
                    builder.OpenElement(54, "span");
                    builder.AddAttribute(55, "class", "ma-value");
                    builder.AddContent(56, ma.Value.ToString("N4"));
                    builder.CloseElement();
                    builder.CloseElement();
                }
            }
            else
            {
                builder.OpenElement(57, "p");
                builder.AddAttribute(58, "class", "no-ma-data");
                builder.AddContent(59, "No moving average data available for this period.");
            }

            builder.CloseElement();
        };
    }
}