using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.EspApi;

public record WaterPump(
    [property: JsonPropertyName("id")] 
    int Id,
    [property: JsonPropertyName("autoPumpBegin")]
    int AutoPumpBegin,
    [property: JsonPropertyName("duration")]
    int Duration,
    [property: JsonPropertyName("lastRun")]
    int LastRun,
    [property: JsonPropertyName("lastRunDuration")]
    int LastRunDuration,
    [property: JsonPropertyName("relaisPin")]
    int RelaisPin
)
{
    public required int Duration { get; set; } = Duration;
    public required int AutoPumpBegin { get; set; } = AutoPumpBegin;
};