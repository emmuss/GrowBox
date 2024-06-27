using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.EspApi;

public record WaterPumpsEspRoot(
    [property: JsonPropertyName("me")] string Me,
    [property: JsonPropertyName("timestamp")] int Timestamp,
    [property: JsonPropertyName("pumps")] WaterPump[] Pumps
);