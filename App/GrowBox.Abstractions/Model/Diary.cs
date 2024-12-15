namespace GrowBox.Abstractions.Model;

public record DiaryTimelapse(string Path, DateTime From, DateTime To);
public record Diary(DiarySnapshot[] Snapshots, DiaryTimelapse[] Timelapses, Guid GrowBoxId)
{
    public static readonly Diary Default = new([], [], Guid.Empty);
};