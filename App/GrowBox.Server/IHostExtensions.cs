using GrowBox.Abstractions;

namespace GrowBox.Server;


public static class IHostExtension
{
    public static async Task UseGrowBoxContext(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GrowBoxContext>();

        //await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        if (!dbContext.GrowBoxes.Any())
        {
            dbContext.GrowBoxes.Add(new Abstractions.Model.GrowBox() { Icon = "yard", GrowBoxUrl = "http://192.168.178.183/", WebCamStreamUrl = "http://192.168.178.200:8081/0/stream", WebCamSnapshotUrl = "http://192.168.178.200:8080/0/", Name = "TEST GROWBOX"});
            await dbContext.SaveChangesAsync();
        }
        
        // if (!dbContext.SensorReadings.Any())
        // {
        //     var growBox = dbContext.GrowBoxes.First();
        //     TimeSpan stepSize = TimeSpan.FromMinutes(5);
        //     DateTime now = DateTime.Now;
        //     for (var i = 0; i < 500; i++)
        //     {
        //         now -= stepSize;
        //         dbContext.SensorReadings.Add(new SensorReading() { GrowBoxId = growBox.Id, Type = "temperature", Value = Random.Shared.NextDouble(), Created = now});
        //         dbContext.SensorReadings.Add(new SensorReading() { GrowBoxId = growBox.Id, Type = "humidity", Value = Random.Shared.NextDouble(), Created = now });
        //         dbContext.SensorReadings.Add(new SensorReading() { GrowBoxId = growBox.Id, Type = "light", Value = Random.Shared.NextDouble(), Created = now });
        //         dbContext.SensorReadings.Add(new SensorReading() { GrowBoxId = growBox.Id, Type = "pressure", Value = Random.Shared.NextDouble(), Created = now });
        //     }
        //
        //     dbContext.IsCreationMarkingEnabled = false;
        //     await dbContext.SaveChangesAsync();
        //     dbContext.IsCreationMarkingEnabled = true;
        // }
    }
}