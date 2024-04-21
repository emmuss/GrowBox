using Bogus;
using GrowBox.Navigation;
using GrowBox.State;

namespace GrowBox.Services;

public sealed class GrowBoxPageSubItemsProvider(App app) : ISubItemProvider
{
    public Task<IEnumerable<NavigationItem>> GenerateItems(NavigationItem item)
    {
        var items = 
            app.AppStateCurrent.GrowBoxes.Select(x => 
                new NavigationItem(
                    Text: x.GrowBox.Name, 
                    Route:$"/growbox/{x.GrowBox.Id}",
                    Icon: x.GrowBox.Icon ?? "",
                    Image: x.GrowBox.Image ?? ""
                )
            ).ToList();
        items.Add(new NavigationItem("GrowBox", "/growbox/create", "add_circle"));
        return Task.FromResult<IEnumerable<NavigationItem>>(items);
    }
}