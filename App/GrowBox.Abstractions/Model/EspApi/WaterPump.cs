using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.EspApi;

public record WaterPump(
    [property: JsonPropertyName("id")] 
    int Id,
    int AutoPumpBegin,
    int Duration,
    [property: JsonPropertyName("lastRun")]
    int LastRun,
    [property: JsonPropertyName("lastRunDuration")]
    int LastRunDuration,
    [property: JsonPropertyName("relaisPin")]
    int RelaisPin,
    [property: JsonPropertyName("isPumpActive")]
    bool IsPumpActive
)
{
    [property: JsonPropertyName("duration")]
    public required int Duration { get; set; } = Duration;
    [property: JsonPropertyName("autoPumpBegin")]
    public required int AutoPumpBegin { get; set; } = AutoPumpBegin;
};