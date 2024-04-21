using Microsoft.AspNetCore.Components;

namespace GrowBox.Controls.Overlay;

internal class OverlayInstance 
{
    public RenderFragment RenderFragment { get; set; } 
    public Guid OverlayInstanceId { get; set; }
    public TaskCompletionSource<OverlayResult<object>> TaskCompletionSource { get; set; } = new();
    public IOverlayBase? Overlay { get; set; }
    public OverlayInstance(RenderFragment renderFragment, Guid overlayInstanceId)
    {
        RenderFragment = renderFragment;
        OverlayInstanceId = overlayInstanceId;
    }
}
