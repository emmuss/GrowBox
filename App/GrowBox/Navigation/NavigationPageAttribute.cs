
namespace GrowBox.Navigation;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class NavigationPageAttribute : Attribute
{
    public NavigationPageAttribute(string text, string icon = "", int sort = 9999, Type? subItemProvider = null, string image = "")
    {
        Text = text;
        Icon = icon;
        Sort = sort;
        Image = image;
        SubItemProvider = subItemProvider;
    }

    public string Image { get; private set; }
    public string Text { get; private set; }
    public string Icon { get; private set; }
    public int Sort { get; private set; }
    public Type? SubItemProvider { get; private set; }
}

public record NavigationItem(string Text, string Route, string Icon = "", int Sort = 9999, Type? SubItemProvider = null, string Image = "")
{
    public List<NavigationItem> Items { get; set; } = new List<NavigationItem>();
}

public interface ISubItemProvider
{
    public Task<IEnumerable<NavigationItem>> GenerateItems(NavigationItem item);
}

public interface INavigationPanelService
{
    void SetNavigationPanel(NavigationPanel panel);

    Task UpdateItemsFor(Type? providerType = null);
}

public class NavigationPanelService : INavigationPanelService
{
    private NavigationPanel? _panel;

    public void SetNavigationPanel(NavigationPanel panel)
    {
        _panel = panel;
    }

    public async Task UpdateItemsFor(Type? providerType = null)
    {
        if (_panel == null) return;
        await _panel.UpdateItemsFor(providerType);
    }
}