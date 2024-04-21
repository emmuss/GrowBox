using System.ComponentModel.DataAnnotations;

namespace GrowBox.Abstractions.Model;

public class SensorReading : IEntityId, IEntityCreated
{
    [Key]
    public Guid Id { get; set; } = Guid.Empty;
    /// <summary>
    /// FOREIGN KEY <see cref="GrowBox"/>.
    /// </summary>
    public Guid GrowBoxId { get; set; }
    
    /// <summary>
    /// Type of the sensor reading.
    /// </summary>
    [StringLength(16)]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Value of sensor reading.
    /// </summary>
    public double Value { get; set; } = 0d;
    
    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime Created { get; set; } = DateTime.MinValue;
    
}