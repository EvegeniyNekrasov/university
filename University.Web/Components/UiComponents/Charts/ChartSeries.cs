namespace University.Web.Components.UiComponents.Charts;

public sealed class ChartSeries
{
    public string Name { get; set; } = "Serie";
    public string? Type { get; set; }     // line, area, column, bar, spline
    public string? Stack { get; set; }    // opcional para varias pilas
    public string? Color { get; set; }
    public bool? Visible { get; set; }
    public List<decimal?> Data { get; set; } = [];
}