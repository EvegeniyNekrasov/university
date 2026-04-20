using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace University.Web.Components.UiComponents.Charts;

public abstract class HighchartsComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected IJSRuntime Js { get; set; } = default;
    private const int DEFAULT_CHART_HEIGHT = 400;

    private IJSObjectReference? _module;
    private string? _lastSerializedConfig;

    protected string ContainerId { get; } = $"hc_{Guid.NewGuid():N}";
    protected abstract string ModulePath { get; }
    protected abstract object BuildConfig();

    protected static readonly JsonSerializerOptions JsonOption = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var config = BuildConfig();
        var serialized = JsonSerializer.Serialize(config, JsonOption);

        if (firstRender)
        {
            _module = await Js.InvokeAsync<IJSObjectReference>("import", ModulePath);
            await _module.InvokeVoidAsync("render", ContainerId, config);
            _lastSerializedConfig = serialized;
            return;
        }

        if (_module is not null && serialized != _lastSerializedConfig)
        {
            await _module.InvokeVoidAsync("update", ContainerId, config);
            _lastSerializedConfig = serialized;
        }
    }

    protected static int ParseHeightPx(string? height)
    {
        if (string.IsNullOrWhiteSpace(height))
        {
            return DEFAULT_CHART_HEIGHT;
        }

        if (height.EndsWith("px", StringComparison.OrdinalIgnoreCase) && int.TryParse(height[..^2], out var px))
        {
            return px;
        }

        return DEFAULT_CHART_HEIGHT;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("destroy", ContainerId);
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
    }

}