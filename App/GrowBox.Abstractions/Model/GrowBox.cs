using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrowBox.Abstractions.Model;

public class GrowBox : IEntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime Created { get; set; } = DateTime.MinValue;
    public DateTime Updated { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Display name of the GrowBox.
    /// </summary>
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Icon of the GrowBox (https://fonts.google.com/icons), displayed when no image is set.
    /// </summary>
    [StringLength(64)]
    public string? Icon { get; set; } = string.Empty;
    /// <summary>
    /// Image of the GrowBox.
    /// </summary>
    [StringLength(1024)]
    public string? Image { get; set; } = string.Empty;

    /// <summary>
    /// Url to the ESP controlling the growbox.
    /// </summary>
    [StringLength(1024)]
    public string GrowBoxUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Web Cam Stream Url "http://growbox-webcam-1:8080/?action=stream"
    /// </summary>
    [StringLength(1024)]
    public string? WebCamStreamUrl { get; set; } = string.Empty;
    /// <summary>
    /// Motion Api Url "http://growbox-webcam-1:8080/?action=snapshot"
    /// </summary>
    [StringLength(1024)]
    public string? WebCamSnapshotUrl { get; set; } = string.Empty;

}