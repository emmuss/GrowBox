namespace GrowBox.Controls.Overlay;

public record OverlayResult(bool Success, object? Value = null);
public record OverlayResult<TData>(bool Success, TData? Data = default)
: OverlayResult(Success, Data);
