using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using GrowBox.Abstractions.Model.GrowBoxAPI;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.Services;

public class RetentionService(ServerConfiguration config, IServiceProvider services, ILogger<RetentionService> logger) : BackgroundService
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

            var purgeUntil = DateTime.UtcNow - config.SensorReadingsRetention;
            logger.LogInformation($"Begin purge until {purgeUntil}, Retention = {config.SensorReadingsRetention}.");
            var purged = await context.SensorReadings.Where(x => x.Created < purgeUntil).ExecuteDeleteAsync(cancellationToken);
            logger.LogInformation($"End purge, count = {purged}");
            
            logger.LogInformation("Begin gathering.");
            foreach (var growBox in await context.GrowBoxes.ToArrayAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested) break;
                var api = new GrowBoxApiService(http, growBox.GrowBoxUrl);
                GrowBoxRoot? root = null;
                
                for (int i = 1; i <= 5; i++)
                {
                    try
                    {
                        root = await api.Get(cancellationToken);
                        if (root is not null)
                            break;
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning(e, $"Gather try {i} for growbox {growBox.Name} failed.");
                    }
                }

                if (root == null)
                {
                    logger.LogError($"Gathering failed for growbox {growBox.Name}.");
                    continue;
                }

                context.SensorReadings.Add(new SensorReading { GrowBoxId = growBox.Id, 
                    Type = "temperature", Value = root.Bme.Temperature, });
                context.SensorReadings.Add(new SensorReading { GrowBoxId = growBox.Id, 
                    Type = "pressure", Value = root.Bme.Pressure, });
                context.SensorReadings.Add(new SensorReading { GrowBoxId = growBox.Id, 
                    Type = "humidity", Value = root.Bme.Humidity, });
                isContextChanged = true;
                logger.LogInformation($"Gathering completed for growbox {growBox.Name}.");
            }

            if (isContextChanged)
                await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("End gathering.");
            
            // wait for next execution.
            await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
        }

        logger.LogInformation("Stopped.");
    }
}

