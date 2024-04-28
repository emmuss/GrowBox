using System.ComponentModel.DataAnnotations;

namespace GrowBox.Abstractions.Model;

public record GrowDiaryNoteMediaRequest(Guid GrowDiaryNoteId);
public class GrowDiaryNoteMedia : IEntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime Created { get; set; } = DateTime.MinValue;
    public DateTime Updated { get; set; } = DateTime.MinValue;

    /// <summary>
    /// FOREIGN KEY <see cref="GrowDiaryNote"/>.
    /// </summary>
    public Guid GrowDiaryNoteId { get; set; }
    
    [StringLength(1024)]
    public string? Name { get; set; }
    
    [StringLength(64)]
    public string? MimeType { get; set; }

    public byte[] Data { get; set; } = [];
}