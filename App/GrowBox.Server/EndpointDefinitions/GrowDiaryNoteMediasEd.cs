using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.EndpointDefinitions;

public class GrowDiaryNoteMediasEd : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapCreate<GrowDiaryNoteMedia, GrowDiaryNoteMedia>(async (GrowDiaryNoteMedia entity, GrowBoxContext ctx) =>
        {
            await ctx.GrowDiaryNoteMedias.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity;
        });
        builder.MinimalMapUpdate<GrowDiaryNoteMedia, GrowDiaryNoteMedia>(async (GrowDiaryNoteMedia entity, GrowBoxContext ctx) =>
        {
            ctx.GrowDiaryNoteMedias.Update(entity);
            await ctx.SaveChangesAsync();
            return entity;
        });
        builder.MinimalMapDelete<GrowDiaryNoteMedia, GrowDiaryNoteMedia>((GrowDiaryNoteMedia entity, GrowBoxContext ctx) =>
        {
            var count = ctx.GrowDiaryNoteMedias.Where(x => x.Id == entity.Id).ExecuteDelete();
            return count > 0 ? entity : new GrowDiaryNoteMedia();
        });
        builder.MinimalMapGet<GrowDiaryNoteMediaRequest, GrowDiaryNoteMedia[]>( (GrowDiaryNoteMediaRequest request, GrowBoxContext ctx) =>
        {
            return ctx.GrowDiaryNoteMedias.Where(x => x.GrowDiaryNoteId == request.GrowDiaryNoteId).ToArrayAsync();
        });
    }
}