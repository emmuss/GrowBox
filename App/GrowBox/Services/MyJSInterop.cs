using Microsoft.JSInterop;

namespace GrowBox.Services;

public class MyJsInterop
{
    private readonly IJSRuntime _jsRuntime;

    public MyJsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ValueTask OpenUrl(string url, string target = "_blank") => _jsRuntime.InvokeVoidAsync("open", url, target);
}