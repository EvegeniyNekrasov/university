namespace University.Web.Components.UiComponents.Charts;

public sealed class PiePoint
{
    public string Name { get; set; } = string.Empty;
    public decimal Y { get; set; }
    public bool? Sliced { get; set; }
    public bool? Selected { get; set; }
    public string? Color { get; set; }
}