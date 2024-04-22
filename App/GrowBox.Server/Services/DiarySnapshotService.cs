using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using GrowBox.Abstractions.Model.EspApi;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.Services;

public class DiarySnapshotService(ServerConfiguration config, IServiceProvider services, ILogger<DiarySnapshotService> logger) : BackgroundService
{
    public const string DIARY_SNAPSHOT_FILE_MARKER = "gbd-";
    public const string DIARY_SNAPSHOT_FILE_DATETIME_MASK = "yyyy-MM-ddTHH-mm-ss";
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Started.");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            // create a scope and get dependencies
            using var scope = services.CreateScope();
            var isContextChanged = false;
            var context = scope.ServiceProvider.GetRequiredService<GrowBoxContext>();
            var http = scope.ServiceProvider.GetRequiredService<HttpClient>();
            
            logger.LogInformation("Begin snap shooting.");
            foreach (var growBox in await context.GrowBoxes.ToArrayAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) break;
                var snapshotUrl = growBox.WebCamSnapshotUrl;
                // TODO: enable disable.
                if (string.IsNullOrEmpty(snapshotUrl))
                    continue;
                
                int? growBoxLight = null;
                try
                {
                    var api = new EspApiService(http, growBox.GrowBoxUrl);
                    var root = await api.Get(cancellationToken);
                    growBoxLight = root?.Light;
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error getting light state for {growBox.Name}. (Url: {growBox.GrowBoxUrl})");
                }

                if (growBoxLight == 255)
                {
                    continue;
                }
                
                var compressedGuidForPath =  growBox.Id.ToBase64AsFileName();
                logger.LogInformation($"Compressed Guid for path: {compressedGuidForPath}");
                var snapshotTargetDir = Path.Combine(config.DiarySnapshotOutputPath, compressedGuidForPath);
                var whoAmIFilePath = Path.Combine(snapshotTargetDir, "growbox.txt");
                var snapshotFilePath = Path.Combine(snapshotTargetDir, 
                    DIARY_SNAPSHOT_FILE_MARKER + DateTime.Now.ToString(DIARY_SNAPSHOT_FILE_DATETIME_MASK)+".jpeg");

                try
                {
                    if (!Directory.Exists(snapshotTargetDir))
                        Directory.CreateDirectory(snapshotTargetDir);

                    await File.WriteAllTextAsync(whoAmIFilePath, $"{growBox.Id}:{growBox.Name}", cancellationToken);

                    var snapshotBytes = await http.GetByteArrayAsync(snapshotUrl, cancellationToken);
                    await File.WriteAllBytesAsync(snapshotFilePath, snapshotBytes, cancellationToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error requesting snapshot for {growBox.Name}. (Url: {snapshotUrl})");
                }
            }

            if (isContextChanged)
                await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("End snap shooting.");
            
            // wait for next execution.
            await Task.Delay(TimeSpan.FromMinutes(60), cancellationToken);
        }

        logger.LogInformation("Stopped.");
    }
}