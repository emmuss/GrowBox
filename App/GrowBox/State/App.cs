using System.Reactive.Linq;
using System.Reactive.Subjects;
using Blazor.MinimalApi.Client;
using GrowBox.Controls.Overlay;
using GrowBox.Controls.Overlays;
using GrowBox.Navigation;
using GrowBox.Services;

namespace GrowBox.State;

public class App
{
    private readonly MinimalHttpClient<Abstractions.Model.GrowBoxModel[]> _growBoxRead;
    private readonly MinimalHttpClient<Abstractions.Model.GrowBoxModel, Abstractions.Model.GrowBoxModel> _growBoxWrite;
    private readonly INavigationPanelService _navigationPanelService;
    private readonly ILogger<App> _logger;
    private readonly BehaviorSubject<AppState> _appStateSubject = new(State.AppState.DEFAULT);
    public IObservable<AppState> AppState { get; }
    public AppState AppStateCurrent => _appStateSubject.Value;
    
    public App(
        MinimalHttpClient<Abstractions.Model.GrowBoxModel[]> growBoxRead, 
        MinimalHttpClient<Abstractions.Model.GrowBoxModel, Abstractions.Model.GrowBoxModel> growBoxWrite,
        INavigationPanelService navigationPanelService,
        ILogger<App> logger)
    {
        _growBoxRead = growBoxRead;
        _growBoxWrite = growBoxWrite;
        _navigationPanelService = navigationPanelService;
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

    public AppGrowBox GetAppGrowBox(Guid id)
        => AppStateCurrent.GrowBoxes.FirstOrDefault(x => x.GrowBox.Id == id)?? throw new InvalidOperationException();

    public async Task<Abstractions.Model.GrowBoxModel> CreateGrowBox(Abstractions.Model.GrowBoxModel growBox)
    {
        var result = await _growBoxWrite.Create(growBox);

        if (result == null) throw new Exception("GrowBox creation failed.");

        await UpdateAppState();
        
        return result;
    }
    public async Task<Abstractions.Model.GrowBoxModel> UpdateGrowBox(Abstractions.Model.GrowBoxModel growBox)
    {
        var result = await _growBoxWrite.Update(growBox);

        if (result == null) throw new Exception("GrowBox update failed.");

        await UpdateAppState();
        
        return result;
    }
    
    public async Task<Abstractions.Model.GrowBoxModel> DeleteGrowBox(Abstractions.Model.GrowBoxModel growBox)
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
            var growBoxes = await _growBoxRead.Get(cancellationToken: cancellationToken) ?? Array.Empty<Abstractions.Model.GrowBoxModel>();
            _appStateSubject.OnNext(_appStateSubject.Value with
            {
                GrowBoxes = growBoxes.Select(x => new AppGrowBox(x)).ToArray(),
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