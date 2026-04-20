namespace University.Web.Components.UiComponents.Charts;

public sealed class StockSeries
{
    public string Name { get; set; } = "Serie";
    public string? Type { get; set; }
    public List<StockPoint> Data { get; set; } = [];
}