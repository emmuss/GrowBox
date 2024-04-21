using GrowBox.Abstractions;
using Microsoft.EntityFrameworkCore;

using GrowBoxModel = GrowBox.Abstractions.Model.GrowBox;

namespace GrowBox.Server.EndpointDefinitions;

public class GrowBoxEd : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapCreate<GrowBoxModel, GrowBoxModel>(async (GrowBoxModel newGrowBox, GrowBoxContext ctx) =>
        {
            await ctx.GrowBoxes.AddAsync(newGrowBox);
            await ctx.SaveChangesAsync();
            return newGrowBox;
        });
        builder.MinimalMapDelete<GrowBoxModel, GrowBoxModel>((GrowBoxModel req, GrowBoxContext ctx) =>
        {
            var count = ctx.GrowBoxes.Where(x => x.Id == req.Id).ExecuteDelete();
            return count > 0 ? req : new GrowBoxModel();
        });
        builder.MinimalMapGet<GrowBoxModel[]>((GrowBoxContext ctx) => ctx.GrowBoxes.ToArrayAsync());
    }
}