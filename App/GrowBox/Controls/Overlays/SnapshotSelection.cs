using GrowBox.Abstractions.Model;

namespace GrowBox.Controls.Overlays;

public class SnapshotSelection(Guid growBoxId)
{
    public Guid GrowBoxId { get; set; } = growBoxId;
    public DiarySnapshot? DiarySnapshot { get; set; }
}