using Microsoft.AspNetCore.Components;

namespace GrowBox.Controls.Overlay;

public class OverlayService : IOverlayService
{
    private IOverlayHost? _overlayHost;

    private IList<OverlayInstance> GetOverlayInstances() => _overlayHost?.OverlayInstances ?? throw new InvalidOperationException("No OverlayHost found.");

    public void CloseOverlay(IOverlayBase overlay, OverlayResult result)
    {
        var overlays = GetOverlayInstances();
        var overlayInstance = overlays.FirstOrDefault(oi => oi.OverlayInstanceId == overlay.InstanceId);
        if (overlayInstance == null)
            return;
        overlays.Remove(overlayInstance);
        overlayInstance.TaskCompletionSource.SetResult(new OverlayResult<object>(result.Success, result.Value));
    }

    public async Task<OverlayResult> ShowOverlay<TOverlay>() where TOverlay : OverlayBase
    {
        var overlays = GetOverlayInstances();
        OverlayInstance? overlayInstance = null;
        Guid instanceId = Guid.NewGuid();
        var renderFragment = new RenderFragment(builder => {
            builder.OpenComponent<TOverlay>(0);
            builder.SetKey("hktOverlayInstance_" + instanceId);
            builder.AddAttribute(1, "InstanceId", instanceId);
            builder.AddAttribute(2, "OverlayService", this);
            builder.AddComponentReferenceCapture(3, compRef => overlayInstance!.Overlay = (OverlayBase)compRef);
            builder.CloseComponent();
        });
        overlayInstance = new OverlayInstance(renderFragment, instanceId);
        overlays.Add(overlayInstance);
        var result = await overlayInstance.TaskCompletionSource.Task;
        return new OverlayResult(result.Success);
    }

    public async Task<OverlayResult<TOverlayData>> ShowOverlay<TOverlay, TOverlayData>(TOverlayData? overlayData) 
        where TOverlay : OverlayDataBase<TOverlayData>
    {
        var overlays = GetOverlayInstances();
        OverlayInstance? overlayInstance = null;
        Guid instanceId = Guid.NewGuid();
        var renderFragment = new RenderFragment(builder => {
            builder.OpenComponent<TOverlay>(0);
            builder.SetKey("hktOverlayInstance_" + instanceId);
            builder.AddAttribute(1, "OverlayData", overlayData);
            builder.AddAttribute(2, "InstanceId", instanceId);
            builder.AddAttribute(3, "OverlayService", this);
            builder.AddComponentReferenceCapture(4, compRef => overlayInstance!.Overlay = (OverlayDataBase<TOverlayData>)compRef);
            builder.CloseComponent();
        });
        overlayInstance = new OverlayInstance(renderFragment, instanceId);
        overlays.Add(overlayInstance);
        var result = await overlayInstance.TaskCompletionSource.Task;
        return new OverlayResult<TOverlayData>(result.Success, (TOverlayData)result.Data!);
    }

    void IOverlayService.SetOverlayHost(IOverlayHost overlayHost) => _overlayHost = overlayHost;
}
