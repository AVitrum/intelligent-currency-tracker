using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace UI.Components.Chart;

public partial class MiniChart : ComponentBase
{
    [Parameter]
    public string Currency { get; set; } = string.Empty;

    [Parameter]
    public decimal[] Data { get; set; } = [];

    [Parameter]
    public string[] Dates { get; set; } = [];

    [Parameter]
    public EventCallback OnRemove { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Data.Length != 0 && Dates.Length != 0)
        {
            await JS.InvokeVoidAsync("drawMiniChart",
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
            await OnRemove.InvokeAsync(null);
        }
    }
}