using System.Reactive.Linq;
using System.Reactive.Subjects;
using GrowBox.Abstractions.Model.EspApi;

namespace GrowBox.Services;

public sealed class GrowBoxServiceFactory(HttpClient http, ILogger<GrowBoxService> logger)
{
    public GrowBoxService Create(Abstractions.Model.GrowBox growBox) =>
        new GrowBoxService(http, logger, growBox);
}

public sealed class GrowBoxService : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly ILogger<GrowBoxService> _logger;
    private readonly Abstractions.Model.GrowBox _growBox;
    private readonly Subject<EspRoot?> _growBoxRootSub = new();
    public IObservable<EspRoot?> GrowBoxRoot => _growBoxRootSub.AsObservable();

    private Task? _updateTask;
    private CancellationTokenSource? _cancellationTokenSource;

    private readonly List<IDisposable> _disposables = new();
    
    private readonly Subject<int> _updateFanSpeedSub = new();
    private readonly Subject<int> _updateLightSub = new();
    private readonly Subject<LightScheduleReq> _updateSunScheduleSub = new();
    private readonly EspApiService _espApi;


    public GrowBoxService(HttpClient http, ILogger<GrowBoxService> logger, Abstractions.Model.GrowBox growBox)
    {
        _espApi = new EspApiService(http, growBox.GrowBoxUrl);
        _http = http;
        _logger = logger;
        _growBox = growBox;

        _disposables.Add(_updateFanSpeedSub.Throttle(TimeSpan.FromSeconds(1)).Subscribe(async speed => {
            try
            {
                _logger.LogInformation($"Updating Fan Speed to {speed}");
                var root = await _espApi.FanSet(speed);
                _growBoxRootSub.OnNext(root);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update FanSpeed Failed.");
            }
        }));
        _disposables.Add(_updateLightSub.Throttle(TimeSpan.FromSeconds(1)).Subscribe(async light => {
            try
            {
                
                _logger.LogInformation($"Updating Light to {light}");
                var root = await _espApi.LightSet(light);
                _growBoxRootSub.OnNext(root);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Light Failed.");
            }
        }));
        _disposables.Add(_updateSunScheduleSub.Distinct().Throttle(TimeSpan.FromSeconds(1)).Subscribe(async x => {
            try
            {
                
                _logger.LogInformation($"Updating light schedule");
                var root = await _espApi.LightScheduleSet(x);
                _growBoxRootSub.OnNext(root);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update Light Failed.");
            }
        }));
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
            var resp = await _espApi.Get();
            if (resp != null) 
            {
                _growBoxRootSub.OnNext(resp);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update Failed.");
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

    public void UpdateLight(int lightValue, bool sunScheduleEnabled, int rise, int duration)
    {
        if (!sunScheduleEnabled) 
        {
            _updateLightSub.OnNext(lightValue);
        }

        _updateSunScheduleSub.OnNext(new (sunScheduleEnabled, rise, duration, lightValue));
    }
    public void UpdateFanSpeed(int value)
    {
        _updateFanSpeedSub.OnNext(value);
    }


    public async ValueTask DisposeAsync()
    {
        await Stop();
        _disposables.ForEach(disposable => disposable.Dispose());
    }
}