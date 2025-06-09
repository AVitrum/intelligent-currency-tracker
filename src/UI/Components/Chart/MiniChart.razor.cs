using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.Services;

namespace UI.Components.Chart;

public partial class MiniChart : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private LocalizationService Localizer { get; set; } = null!;
    [Inject] private UserSettingsService UserSettingsService { get; set; } = null!;

    [Parameter] public string Currency { get; set; } = string.Empty;
    [Parameter] public decimal[] Data { get; set; } = [];
    [Parameter] public string[] Dates { get; set; } = [];
    [Parameter] public EventCallback OnRemove { get; set; }
    [Parameter] public string StartDate { get; set; } = string.Empty;
    [Parameter] public string EndDate { get; set; } = string.Empty;

    private string _viewDetailsButtonText = "";
    private string _removeButtonAriaLabel = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalizedStringsAsync();
        UserSettingsService.OnSettingsChangedAsync += HandleSettingsChangedAsync;
    }

    private async Task LoadLocalizedStringsAsync()
    {
        _viewDetailsButtonText = await Localizer.GetStringAsync("minichart.button.view_details");
        _removeButtonAriaLabel = await Localizer.GetStringAsync("minichart.button.remove_currency_aria_label");
    }

    private async Task HandleSettingsChangedAsync()
    {
        await LoadLocalizedStringsAsync();
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Data.Length != 0 && Dates.Length != 0)
        {
            await Js.InvokeVoidAsync("drawMiniChart",
                $"miniChart-{Currency}",
                $"miniTooltip-{Currency}",
                Data,
                Dates);
        }
    }

    private async Task Remove()
    {
        if (OnRemove.HasDelegate)
        {
            await OnRemove.InvokeAsync(Currency);
        }
    }

    private void NavigateToDetails()
    {
        if (!string.IsNullOrEmpty(Currency) && !string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
        {
            NavigationManager.NavigateTo($"/currency/{Currency}/{StartDate}/{EndDate}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        UserSettingsService.OnSettingsChangedAsync -= HandleSettingsChangedAsync;
        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }
}