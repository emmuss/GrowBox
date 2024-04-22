namespace GrowBox.Abstractions.Model;

public record Diary(DiarySnapshot[] Snapshots, Guid GrowBoxId)
{
    public static readonly Diary Default = new([], Guid.Empty);
};