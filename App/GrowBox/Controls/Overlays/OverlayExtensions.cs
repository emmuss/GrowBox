using GrowBox.Controls.Overlay;
using Radzen;

namespace GrowBox.Controls.Overlays;

public static class OverlayExtensions
{
    public static async Task<bool> ConfirmDelete(this IOverlayService service, string deleteWhat)
    {
        Confirm confirm = new()
        {
            Caption = $"Delete {deleteWhat}",
            Message = $"Are you sure you want to delete {deleteWhat}?",
            FailButtonStyle = ButtonStyle.Light,
            FailText = "Cancel",
            FailIcon = "",
            SuccessIcon = "delete_forever",
            SuccessText = "Delete",
            SuccessButtonStyle = ButtonStyle.Danger,
        };

        var result = await service.ShowOverlay<ConfirmOverlay, Confirm>(confirm);
        Console.WriteLine("wtf {0}", result.Success);
        return result.Success;
    }
}