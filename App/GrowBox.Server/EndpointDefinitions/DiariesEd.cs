using System.Globalization;
using GrowBox.Abstractions;
using GrowBox.Abstractions.Model;
using GrowBox.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace GrowBox.Server.EndpointDefinitions;

public class DiariesEd(ILogger<DiariesEd> logger, ServerConfiguration config) : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder builder)
    {
        builder.MinimalMapGet<DiaryRequest, Diary>(
        async (
            DiaryRequest request, 
            GrowBoxContext ctx
        ) => {
            var growBox = await ctx.GrowBoxes.FirstOrDefaultAsync(x => x.Id == request.GrowBoxId);
            if (growBox == null) return Diary.Default;
            var compressedGuidForPath =  growBox.Id.ToBase64AsFileName();
            
            var snapshotTargetDir = Path.Combine(config.DiarySnapshotOutputPath, compressedGuidForPath);
            List<DiarySnapshot> snapshots = [];
            try
            {
                var files = Directory.GetFiles(
                    snapshotTargetDir, 
                    $"{DiarySnapshotService.DIARY_SNAPSHOT_FILE_MARKER}*");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var dateTimePart = Path.GetFileNameWithoutExtension(file)
                        .Substring(DiarySnapshotService.DIARY_SNAPSHOT_FILE_MARKER.Length);
                    if (string.IsNullOrEmpty(dateTimePart))
                    {
                        continue;
                    }

                    if (!DateTime.TryParseExact(
                            dateTimePart,
                            DiarySnapshotService.DIARY_SNAPSHOT_FILE_DATETIME_MASK,
                            null,
                            DateTimeStyles.AssumeLocal,
                            out var dateTime))
                    {
                        continue;
                    }

                    var snapshot = new DiarySnapshot(
                        $"/snapshot/{compressedGuidForPath}/{fileName}",
                        dateTime);
                    
                    snapshots.Add(snapshot);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, $"Can't build Diary for {growBox.Name}.");
            }

            return new Diary(snapshots.ToArray(), growBox.Id);
        });

        builder.MapGet("/snapshot/{compressedGuidForPath}/{fileName}",
        (
            string compressedGuidForPath, 
            string fileName
        ) => {
            var snapshotTargetDir = Path.Combine(config.DiarySnapshotOutputPath, compressedGuidForPath);
            var snapshotFilePath = Path.Combine(snapshotTargetDir, fileName);
            if (!File.Exists(snapshotFilePath)) 
                return Results.NotFound();
            var fs = File.OpenRead(snapshotFilePath);   
            return Results.File(fs, "image/jpeg", fileName);

        });
    }
}