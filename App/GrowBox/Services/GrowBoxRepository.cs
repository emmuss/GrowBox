using System.Reactive.Linq;
using System.Reactive.Subjects;
using GrowBox.Abstractions.Model.EspApi;
using Refit;

namespace GrowBox.Services;

public sealed class GrowBoxRepository : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<GrowBoxRepository> _logger;
    private readonly Abstractions.Model.GrowBox _growBox;
    private readonly Subject<GrowBoxEspRoot?> _growBoxRootSub = new();
    private readonly Subject<WaterPumpsEspRoot?> _waterPumpsRootSub = new();
    public IObservable<GrowBoxEspRoot?> GrowBoxRoot => _growBoxRootSub.AsObservable();
    public IObservable<WaterPumpsEspRoot?> WaterPumpsRoot => _waterPumpsRootSub.AsObservable();

    private Task? _updateTask;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly List<IDisposable> _disposables = new();
    private readonly IGrowBoxEsp _espGrowBox;
    private readonly IWaterPumpsEsp _espWaterPumps;

    public IGrowBoxEsp GrowBoxEsp => _espGrowBox;
    public IWaterPumpsEsp WaterPumpsEsp => _espWaterPumps;


    public GrowBoxRepository(HttpClient http, ILogger<GrowBoxRepository> logger, Abstractions.Model.GrowBox growBox)
    {
        _espGrowBox = RestService.For<IGrowBoxEsp>(growBox.GrowBoxUrl);
        // TODO: make configurable...
        _espWaterPumps = RestService.For<IWaterPumpsEsp>("http://192.168.178.194");
        _http = http;
        _logger = logger;
        _growBox = growBox;
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

    public async void UpdateAsync()
    {
        await Update();
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
        try
        {
            var resp = await _espWaterPumps.Get();
            _waterPumpsRootSub.OnNext(resp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Waterpumps Update Failed.");
        }
    }
    
    public async Task Snapshot()
    {
        try
        {
            var url = Path.Combine(_growBox.WebCamSnapshotUrl ?? throw new Exception("No Api"), "/action/snapshot");
            var resp = await _http.GetAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Snapshot Failed.");
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await Stop();
        _disposables.ForEach(disposable => disposable.Dispose());
    }
}