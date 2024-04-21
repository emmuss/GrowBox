using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
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
    }
}