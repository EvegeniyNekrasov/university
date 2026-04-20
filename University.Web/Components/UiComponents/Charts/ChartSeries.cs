namespace University.Web.Components.UiComponents.Charts;

public sealed class ChartSeries
{
    public string Name { get; set; } = "Serie";
    public string? Type { get; set; }
    public List<decimal?> Data { get; set; } = [];
}