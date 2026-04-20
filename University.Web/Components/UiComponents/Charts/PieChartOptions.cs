

namespace University.Web.Components.UiComponents.Charts;


public sealed class PieChartOptions
{
    public string Height { get; set; } = "400px";
    public string? Subtitle { get; set; }

    public bool ShowLegend { get; set; } = true;
    public bool ShowDataLabels { get; set; } = true;

    // "60%" => donut
    public string? InnerSize { get; set; }

    public string? TooltipPointFormat { get; set; }
    public string? TooltipValuePrefix { get; set; }
    public string? TooltipValueSuffix { get; set; }

    public IReadOnlyList<string>? Colors { get; set; }
}