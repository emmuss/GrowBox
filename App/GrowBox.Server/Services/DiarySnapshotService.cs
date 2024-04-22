﻿using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using GrowBox.Abstractions.Model.EspApi;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.Services;

public class DiarySnapshotService(ServerConfiguration config, IServiceProvider services, ILogger<DiarySnapshotService> logger) : BackgroundService
{
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
                // TODO: light schedule, enable disable.
                if (string.IsNullOrEmpty(snapshotUrl))
                    continue;
                
                var compressedGuidForPath =Convert.ToBase64String(growBox.Id.ToByteArray())
                    .Trim('=')
                    .Replace('+', '-')
                    .Replace("/", "_");
                logger.LogInformation($"Compressed Guid for path: {compressedGuidForPath}");
                var snapshotTargetDir = Path.Combine(config.DiarySnapshotOutputPath, compressedGuidForPath);
                var whoAmIFilePath = Path.Combine(snapshotTargetDir, "growbox.txt");
                var snapshotFilePath = Path.Combine(snapshotTargetDir, DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")+".jpeg");

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