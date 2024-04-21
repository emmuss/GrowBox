using Microsoft.JSInterop;

namespace GrowBox;

public static class Extensions 
{
    public static TClone Clone<TClone>(this TClone clone) where TClone : class 
    {
        return System.Text.Json.JsonSerializer.Deserialize<TClone>(System.Text.Json.JsonSerializer.Serialize(clone)) ?? throw new ArgumentException("Can't clone.");
    }

    public static ValueTask WindowOpen(this IJSRuntime js, string url, string target = "_blank") 
        => js.InvokeVoidAsync("eval", $"window.open('{url}','{target}')");
}