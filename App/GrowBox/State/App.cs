using System.Reactive.Linq;
using System.Reactive.Subjects;
using Blazor.MinimalApi.Client;
using GrowBox.Navigation;
using GrowBox.Services;

namespace GrowBox.State;

public class App
{
    private readonly MinimalHttpClient<Abstractions.Model.GrowBox[]> _growBoxRead;
    private readonly MinimalHttpClient<Abstractions.Model.GrowBox, Abstractions.Model.GrowBox> _growBoxWrite;
    private readonly INavigationPanelService _navigationPanelService;
    private readonly GrowBoxServiceFactory _growBoxServiceFactory;
    private readonly ILogger<App> _logger;
    private readonly BehaviorSubject<AppState> _appStateSubject = new(State.AppState.DEFAULT);
    public IObservable<AppState> AppState { get; }
    public AppState AppStateCurrent => _appStateSubject.Value;
    
    public App(
        MinimalHttpClient<Abstractions.Model.GrowBox[]> growBoxRead, 
        MinimalHttpClient<Abstractions.Model.GrowBox, Abstractions.Model.GrowBox> growBoxWrite,
        INavigationPanelService navigationPanelService,
        GrowBoxServiceFactory growBoxServiceFactory,
        ILogger<App> logger)
    {
        _growBoxRead = growBoxRead;
        _growBoxWrite = growBoxWrite;
        _navigationPanelService = navigationPanelService;
        _growBoxServiceFactory = growBoxServiceFactory;
        _logger = logger;
        AppState = _appStateSubject.AsObservable();
    }

    public async Task Startup()
    {
        _logger.LogInformation("Begin app startup.");
        // Block everything until app is started.
        while (true)
        {
            var isSuccess = await UpdateAppState();
            if (isSuccess) break;
            await Task.Delay(1000);
        }
        _logger.LogInformation("App startup succeeded.");
    }

    public async Task<Abstractions.Model.GrowBox> CreateGrowBox(Abstractions.Model.GrowBox growBox)
    {
        var result = await _growBoxWrite.Create(growBox);

        if (result == null) throw new Exception("GrowBox creation failed.");

        await UpdateAppState();
        
        return result;
    }
    
    public async Task<Abstractions.Model.GrowBox> DeleteGrowBox(Abstractions.Model.GrowBox growBox)
    {
        var result = await _growBoxWrite.Delete(growBox);

        if (result == null || result.Id == Guid.Empty) throw new Exception("GrowBox deletion failed.");

        await UpdateAppState();
        
        return result;
    }

    public async Task<bool> UpdateAppState(CancellationToken cancellationToken = default)
    {
        try
        {
            var growBoxes = await _growBoxRead.Get(cancellationToken: cancellationToken) ?? Array.Empty<Abstractions.Model.GrowBox>();
            foreach (var appGrowBox in _appStateSubject.Value.GrowBoxes)
            {
                await appGrowBox.Service.DisposeAsync();
            }
            _appStateSubject.OnNext(_appStateSubject.Value with
            {
                GrowBoxes = growBoxes.Select(x => new AppGrowBox(x, _growBoxServiceFactory.Create(x))).ToArray(),
                IsStartup = false,
            });
            await _navigationPanelService.UpdateItemsFor(typeof(GrowBoxPageSubItemsProvider));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "AppState update failed.");
            return false;
        }

        return true;
    }
}