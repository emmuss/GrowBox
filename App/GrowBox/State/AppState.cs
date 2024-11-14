using GrowBox.Services;

namespace GrowBox.State;

public record AppGrowBox(Abstractions.Model.GrowBoxModel GrowBox);

public record AppState(bool IsStartup, AppGrowBox[] GrowBoxes)
{
    public static readonly AppState DEFAULT = new AppState(
        IsStartup: true,
        GrowBoxes: Array.Empty<AppGrowBox>());
};

