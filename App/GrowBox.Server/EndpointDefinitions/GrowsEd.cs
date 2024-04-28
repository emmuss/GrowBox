using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.EndpointDefinitions;

public class GrowsEd : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapCreate<Grow, Grow>(async (Grow entity, GrowBoxContext ctx) =>
        {
            await ctx.Grows.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity;
        });
        builder.MinimalMapUpdate<Grow, Grow>(async (Grow entity, GrowBoxContext ctx) =>
        {
            ctx.Grows.Update(entity);
            await ctx.SaveChangesAsync();
            return ctx.Grows.Include(x => x.GrowDiaryNotes).First(x => x.Id == entity.Id);
        });
        builder.MinimalMapDelete<Grow, Grow>((Grow entity, GrowBoxContext ctx) =>
        {
            var count = ctx.Grows.Where(x => x.Id == entity.Id).ExecuteDelete();
            return count > 0 ? entity : new Grow();
        });
        builder.MinimalMapGet<GrowRequest, Grow[]>( (GrowRequest request, GrowBoxContext ctx) =>
        {
            return ctx.Grows.Where(x => x.GrowBoxId == request.GrowBoxId).ToArrayAsync();
        });
        builder.MinimalMapGet<GrowRequest, Grow>( (GrowRequest request, GrowBoxContext ctx) =>
        {
            return ctx.Grows.Where(x => x.GrowBoxId == request.GrowBoxId).OrderByDescending(x => x.Updated).FirstOrDefaultAsync();
        });
        builder.MinimalMapGet<Grow>((GrowBoxContext ctx, 
                [FromQuery(Name = "id")] Guid id, 
                [FromQuery(Name ="withNavigationProperties")] bool withNavigationProperties = false) 
            => withNavigationProperties 
                ? ctx.Grows.Include(x => x.GrowDiaryNotes).FirstOrDefault(x => x.Id == id)
                : ctx.Grows.FirstOrDefault(x => x.Id == id));
    }
}