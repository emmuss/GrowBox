using Radzen;

namespace GrowBox.Controls.Overlays;

public class Confirm
{
    public string Caption { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SuccessText { get; set; }
    public string? SuccessIcon { get; set; }
    public ButtonStyle SuccessButtonStyle { get; set; } = ButtonStyle.Primary;
    public string? FailText { get; set; }
    public string? FailIcon { get; set; }
    public ButtonStyle FailButtonStyle { get; set; } = ButtonStyle.Warning;
}