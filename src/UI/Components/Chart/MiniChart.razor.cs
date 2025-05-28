using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace UI.Components.Chart;

public partial class MiniChart : ComponentBase
{
    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string Currency { get; set; } = string.Empty;

    [Parameter]
    public decimal[] Data { get; set; } = [];

    [Parameter]
    public string[] Dates { get; set; } = [];

    [Parameter]
    public EventCallback OnRemove { get; set; }

    [Parameter]
    public string StartDate { get; set; } = string.Empty;

    [Parameter]
    public string EndDate { get; set; } = string.Empty;

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
}