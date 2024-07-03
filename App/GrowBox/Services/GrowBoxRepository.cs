using System.Reactive.Linq;
using System.Reactive.Subjects;
using GrowBox.Abstractions.Model.EspApi;
using Refit;

namespace GrowBox.Services;

public sealed class GrowBoxRepository : IAsyncDisposable
{
    private readonly ILogger<GrowBoxRepository> _logger;
    private readonly Subject<GrowBoxEspRoot?> _growBoxRootSub = new();
    public IObservable<GrowBoxEspRoot?> GrowBoxRoot => _growBoxRootSub.AsObservable();

    private Task? _updateTask;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly IGrowBoxEsp _espGrowBox;
    public IGrowBoxEsp GrowBoxEsp => _espGrowBox;

    public GrowBoxRepository(ILogger<GrowBoxRepository> logger, Abstractions.Model.GrowBox growBox)
    {
        _espGrowBox = RestService.For<IGrowBoxEsp>(growBox.GrowBoxUrl);
        _logger = logger;
    }

    public async Task Stop()
    {
        if (_updateTask == null || _cancellationTokenSource == null)
        {
            return;
        }
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        try { await _updateTask.WaitAsync(default(CancellationToken)); }
        catch (Exception) { /* ignored */ }
        _updateTask.Dispose();
        _cancellationTokenSource = null;
        _updateTask = null;
    }

    public void Start() 
    {
        if (_updateTask != null) 
        {
            return;
        }

        _cancellationTokenSource = new();
        _updateTask = Task.Run(async () => {
            while (!_cancellationTokenSource.IsCancellationRequested) 
            {
                await Update();
                await Task.Delay(TimeSpan.FromSeconds(5), _cancellationTokenSource.Token);
            }
        }, _cancellationTokenSource.Token);
    }

    public async Task Update()
    {
        try
        {
            var resp = await _espGrowBox.Get();
            _growBoxRootSub.OnNext(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Growbox Update Failed.");
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await Stop();
    }
}