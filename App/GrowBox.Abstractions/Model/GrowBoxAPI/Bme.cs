using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.GrowBoxAPI;
public record Bme(
    [property: JsonPropertyName("temperature")] double Temperature,
    [property: JsonPropertyName("humidity")] double Humidity,
    [property: JsonPropertyName("pressure")] double Pressure,
    [property: JsonPropertyName("timestamp")] int Timestamp
);