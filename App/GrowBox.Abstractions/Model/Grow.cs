using System.ComponentModel.DataAnnotations;

namespace GrowBox.Abstractions.Model;

public record GrowRequest(Guid GrowBoxId);

public class Grow : IEntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime Created { get; set; } = DateTime.MinValue;
    public DateTime Updated { get; set; } = DateTime.MinValue;
    
    /// <summary>
    /// Name of the grow.
    /// </summary>
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the grow is archived or not.
    /// </summary>
    public bool IsArchived { get; set; } = false;
    
    /// <summary>
    /// FOREIGN KEY <see cref="GrowBox"/>.
    /// </summary>
    public Guid GrowBoxId { get; set; } = Guid.Empty;
    
    /// <summary>
    /// Grow Diary notes.
    /// </summary>
    public ICollection<GrowDiaryNote> GrowDiaryNotes { get; set; } = [];
}