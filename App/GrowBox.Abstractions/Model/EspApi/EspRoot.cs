using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.EspApi
{
    public record EspRoot(
        [property: JsonPropertyName("me")] string Me,
        [property: JsonPropertyName("fanSpeed")] int FanSpeed,
        [property: JsonPropertyName("light")] int Light,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("humidity")] double Humidity,
        [property: JsonPropertyName("pressure")] double Pressure,
        [property: JsonPropertyName("dewPoint")] double DewPoint,
        [property: JsonPropertyName("heatIndex")] double HeatIndex,
        [property: JsonPropertyName("timestamp")] int Timestamp,
        [property: JsonPropertyName("lightSchedule")] LightSchedule LightSchedule
    );
}