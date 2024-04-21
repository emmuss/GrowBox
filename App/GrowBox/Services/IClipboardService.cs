namespace GrowBox.Services;

public interface IClipboardService
{
    Task CopyToClipboard(string text);
}
