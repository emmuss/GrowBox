using GrowBox.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using GrowBoxModel = GrowBox.Abstractions.Model.GrowBox;

namespace GrowBox.Server.EndpointDefinitions;

public class GrowBoxEd : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapCreate<GrowBoxModel, GrowBoxModel>(async (GrowBoxModel growBox, GrowBoxContext ctx) =>
        {
            await ctx.GrowBoxes.AddAsync(growBox);
            await ctx.SaveChangesAsync();
            return growBox;
        });
        builder.MinimalMapUpdate<GrowBoxModel, GrowBoxModel>(async (GrowBoxModel growBox, GrowBoxContext ctx) =>
        {
            ctx.GrowBoxes.Update(growBox);
            await ctx.SaveChangesAsync();
            return growBox;
        });
        builder.MinimalMapDelete<GrowBoxModel, GrowBoxModel>((GrowBoxModel growBox, GrowBoxContext ctx) =>
        {
            var count = ctx.GrowBoxes.Where(x => x.Id == growBox.Id).ExecuteDelete();
            return count > 0 ? growBox : new GrowBoxModel();
        });
        builder.MinimalMapGet<GrowBoxModel[]>((GrowBoxContext ctx) 
            => ctx.GrowBoxes.ToArrayAsync());
        builder.MinimalMapGet<GrowBoxModel>((GrowBoxContext ctx, [FromQuery(Name = "id")] Guid id) 
            => ctx.GrowBoxes.FirstOrDefault(x => x.Id == id));
    }
}