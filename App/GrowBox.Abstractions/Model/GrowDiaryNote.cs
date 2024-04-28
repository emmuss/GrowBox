using System.ComponentModel.DataAnnotations;

namespace GrowBox.Abstractions.Model;

public record GrowDiaryNoteRequest(Guid GrowId);
public class GrowDiaryNote : IEntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime Created { get; set; } = DateTime.MinValue;
    public DateTime Updated { get; set; } = DateTime.MinValue;

    /// <summary>
    /// FOREIGN KEY <see cref="Grow"/>.
    /// </summary>
    public Guid GrowId { get; set; } = Guid.Empty;

    /// <summary>
    /// The type of the note. 
    /// </summary>
    [StringLength(64)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Text.
    /// </summary>
    [StringLength(10000)]
    public string Text { get; set; } = string.Empty;
    
    
    /// <summary>
    /// Grow Diary note media.
    /// </summary>
    public ICollection<GrowDiaryNoteMedia> GrowDiaryNoteMedias { get; } = [];
}