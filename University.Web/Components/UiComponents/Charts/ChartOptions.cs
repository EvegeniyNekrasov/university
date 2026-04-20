namespace University.Web.Components.UiComponents.Charts;

public sealed class ChartOptions
{
    public string Type { get; set; } = "line";
    public string Height { get; set; } = "400px";

    public string? Subtitle { get; set; }

    public string? XAxisTitle { get; set; }
    public string? YAxisTitle { get; set; }

    public int? XAxisLabelsRotation { get; set; }

    public decimal? YAxisMin { get; set; }
    public decimal? YAxisMax { get; set; }

    public bool ShowLegend { get; set; } = true;

    public ChartStackingMode Stacking { get; set; } = ChartStackingMode.None;

    public string? TooltipPointFormat { get; set; }
    public string? TooltipValuePrefix { get; set; }
    public string? TooltipValueSuffix { get; set; }

    public IReadOnlyList<string>? Colors { get; set; }

    public bool EnableDataLabels { get; set; } = false;
}