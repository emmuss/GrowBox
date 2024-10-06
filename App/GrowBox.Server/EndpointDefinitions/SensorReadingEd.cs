using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using GrowBox.Abstractions.Model.EspApi;
using GrowBox.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.EndpointDefinitions;

public class SensorReadingED : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapGet<SensorReadingRequest, SensorReading[]>(async (SensorReadingRequest request, GrowBoxContext ctx) =>
        {
            var readings =
                ctx.SensorReadings.Where(x => 
                    x.GrowBoxId == request.GrowBoxId &&
                    request.Types.Contains(x.Type));
            if (request.From is not null)
                readings = readings.Where(x => x.Created >= request.From);
            if (request.To is not null)
                readings = readings.Where(x => x.Created <= request.To);

            return await readings.OrderByDescending(x => x.Created).ToArrayAsync();
        });
        builder.MapPost("/waterpump/telemetry/{growBoxId}", async ([FromBody]WaterPump waterPump, Guid growBoxId, GrowBoxContext ctx) =>
        {
            if (!ctx.GrowBoxes.Any(x => x.Id == growBoxId))
            {
                throw new ArgumentException("Unknown GrowBox.", nameof(growBoxId));
            }
            var name = (string type) => $"wp{waterPump.Id}_" + type;
            ctx.SensorReadings.Add(new SensorReading() { GrowBoxId = growBoxId, Type = name("active"), Value = waterPump.IsPumpActive ? 1 : 0});

            await ctx.SaveChangesAsync();
        });
    }
}