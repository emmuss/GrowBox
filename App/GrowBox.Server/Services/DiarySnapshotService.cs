using System.Globalization;
using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using GrowBox.Abstractions.Model.EspApi;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace GrowBox.Server.Services;

public class DiarySnapshotService(ServerConfiguration config, IServiceProvider services, ILogger<DiarySnapshotService> logger) : BackgroundService
{
    private GrowBoxImageMutator mutator = new ();
    public const string DIARY_SNAPSHOT_FILE_MARKER = "gbd-";
    public const string DIARY_SNAPSHOT_FILE_EXTENSION = ".jpeg";
    public const string DIARY_TIMELAPSE_FILE_EXTENSION = ".mp4"; 
    public const string DIARY_TIMELAPSE_SNAPSHOT_FILE_EXTENSION = ".webp"; 
    public const string DIARY_TIMELAPSE_DATETIME_SPLITTER = "_to_"; 
    public const string DIARY_SNAPSHOT_FILE_DATETIME_MASK = "yyyy-MM-ddTHH-mm-ss";
    public const string DIARY_SNAPSHOT_FILE_DATETIME_SEAR = "****-**-**T**-**-**";

    public static (string Path, DateTime TimeStamp)[] GetSnapshots(string path)
    {
        return Directory.GetFiles(path,
                DIARY_SNAPSHOT_FILE_MARKER + DIARY_SNAPSHOT_FILE_DATETIME_SEAR + DIARY_SNAPSHOT_FILE_EXTENSION)
            .Select(x => (
                Path: x, 
                Timestamp: DateTime.ParseExact(
                    Path.GetFileNameWithoutExtension(x).Replace(DIARY_SNAPSHOT_FILE_MARKER, ""), 
                    DIARY_SNAPSHOT_FILE_DATETIME_MASK,
                    CultureInfo.InvariantCulture)))
            .OrderBy(x =>x.Timestamp).ToArray();
    }

    public static async Task GenerateWeeklyTimelapses(string path, CancellationToken cancellationToken)
    {
        var snapshots = GetSnapshots(path);
        while (snapshots.Length > 0)
        {
            DateTime weekBegin = snapshots.First().TimeStamp;
            DateTime weekEnd = weekBegin.AddDays(7);
            void TrimSnapshots() {
                snapshots = snapshots.Where(x => x.TimeStamp > weekEnd).ToArray();
            }
            if (weekEnd > DateTime.Now)
                return;
            string timelapseName =
                DIARY_SNAPSHOT_FILE_MARKER +
                weekBegin.ToString(DIARY_SNAPSHOT_FILE_DATETIME_MASK) +
                DIARY_TIMELAPSE_DATETIME_SPLITTER +
                weekEnd.ToString(DIARY_SNAPSHOT_FILE_DATETIME_MASK) +
                DIARY_TIMELAPSE_FILE_EXTENSION;
            string timelapsePath = Path.Combine(path, timelapseName);
            if (File.Exists(timelapsePath))
            {
                TrimSnapshots();
                continue;
            }

            var imagesForTimelapse = snapshots
                .Where(x => x.TimeStamp >= weekBegin && x.TimeStamp < weekEnd)
                .Select(x => x.Path)
                .ToArray(); 
            TrimSnapshots();

            try
            {
                await ImagesToMp4.GenerateTimelapse(
                    timelapsePath,
                    imagesForTimelapse, 
                    cancellationToken,
                    singleImageDuration: TimeSpan.FromSeconds(1d / 4d));
            }
            catch 
            {
                if (File.Exists(timelapsePath))
                {
                    File.Delete(timelapsePath);
                }
                throw;
            }
        }
    }

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
                GrowBoxEspRoot? root = null;
                try
                {
                    var api = new GrowboxEspApiService(http, growBox.GrowBoxUrl);
                    root = await api.Get(cancellationToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error getting light state for {growBox.Name}. (Url: {growBox.GrowBoxUrl})");
                }

                if (root == null || root.Light == 255)
                {
                    continue;
                }
                
                var compressedGuidForPath =  growBox.Id.ToBase64AsFileName();
                var now = DateTime.Now;
                logger.LogInformation($"Compressed Guid for path: {compressedGuidForPath}");
                var snapshotTargetDir = Path.Combine(config.DiarySnapshotOutputPath, compressedGuidForPath);
                var whoAmIFilePath = Path.Combine(snapshotTargetDir, "growbox.txt");
                var snapshotFilePath = Path.Combine(snapshotTargetDir, 
                    DIARY_SNAPSHOT_FILE_MARKER + now.ToString(DIARY_SNAPSHOT_FILE_DATETIME_MASK)+DIARY_SNAPSHOT_FILE_EXTENSION);
                var timelapseSnapshotFilePath = Path.Combine(snapshotTargetDir, 
                    DIARY_SNAPSHOT_FILE_MARKER + now.ToString(DIARY_SNAPSHOT_FILE_DATETIME_MASK)+DIARY_TIMELAPSE_SNAPSHOT_FILE_EXTENSION);
                try
                {
                    if (!Directory.Exists(snapshotTargetDir))
                        Directory.CreateDirectory(snapshotTargetDir);
                    await File.WriteAllTextAsync(whoAmIFilePath, $"{growBox.Id}:{growBox.Name}", cancellationToken);
                    var snapshotBytes = await http.GetByteArrayAsync(snapshotUrl, cancellationToken);
                    await File.WriteAllBytesAsync(snapshotFilePath, snapshotBytes, cancellationToken);
                    using var image = await mutator.Mutate(snapshotBytes, now, root, cancellationToken);
                    await image.SaveAsWebpAsync(timelapseSnapshotFilePath, cancellationToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error requesting snapshot for {growBox.Name}. (Url: {snapshotUrl})");
                }

                try
                {
                    //0
                    //  await GenerateWeeklyTimelapses(snapshotTargetDir, cancellationToken);
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error generating weekly timelapses for {growBox.Name}. targetDir: {snapshotTargetDir}");
                }
            }

            if (isContextChanged)
                await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("End snap shooting.");
            
            // wait for next execution.
            await Task.Delay(TimeSpan.FromMinutes(15), cancellationToken);
        }

        logger.LogInformation("Stopped.");
    }
}