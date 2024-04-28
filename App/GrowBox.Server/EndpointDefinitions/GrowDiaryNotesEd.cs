using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.EndpointDefinitions;

public class GrowDiaryNotesEd : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapCreate<GrowDiaryNote, GrowDiaryNote>(async (GrowDiaryNote entity, GrowBoxContext ctx) =>
        {
            await ctx.GrowDiaryNotes.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity;
        });
        builder.MinimalMapUpdate<GrowDiaryNote, GrowDiaryNote>(async (GrowDiaryNote entity, GrowBoxContext ctx) =>
        {
            ctx.GrowDiaryNotes.Update(entity);
            await ctx.SaveChangesAsync();
            return entity;
        });
        builder.MinimalMapDelete<GrowDiaryNote, GrowDiaryNote>((GrowDiaryNote entity, GrowBoxContext ctx) =>
        {
            var count = ctx.GrowDiaryNotes.Where(x => x.Id == entity.Id).ExecuteDelete();
            return count > 0 ? entity : new GrowDiaryNote();
        });
        builder.MinimalMapGet<GrowDiaryNoteRequest, GrowDiaryNote[]>( (GrowDiaryNoteRequest request, GrowBoxContext ctx) =>
        {
            return ctx.GrowDiaryNotes.Where(x => x.GrowId == request.GrowId).ToArrayAsync();
        });
        builder.MinimalMapGet<GrowDiaryNote>((GrowBoxContext ctx, 
                [FromQuery(Name = "id")] Guid id, 
                [FromQuery(Name ="withNavigationProperties")] bool withNavigationProperties = false) 
            => withNavigationProperties 
                ? ctx.GrowDiaryNotes.Include(x => x.GrowDiaryNoteMedias).FirstOrDefault(x => x.Id == id)
                : ctx.GrowDiaryNotes.FirstOrDefault(x => x.Id == id));
    }
}