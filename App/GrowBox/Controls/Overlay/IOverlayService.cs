namespace GrowBox.Controls.Overlay;

public interface IOverlayService
{
    public void CloseOverlay(IOverlayBase overlay, OverlayResult result);

    public Task<OverlayResult> ShowOverlay<TOverlay>()
        where TOverlay : OverlayBase;

    public Task<OverlayResult<TOverlayData>> ShowOverlay<TOverlay, TOverlayData>(TOverlayData? overlayData) 
        where TOverlay : OverlayDataBase<TOverlayData>;

    internal void SetOverlayHost(IOverlayHost overlayHost);
}
