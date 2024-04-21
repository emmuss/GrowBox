using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.GrowBoxAPI
{
    public record GrowBoxRoot(
        [property: JsonPropertyName("me")] string Me,
        [property: JsonPropertyName("webcam")] string Webcam,
        [property: JsonPropertyName("fsMounted")] string FsMounted,
        [property: JsonPropertyName("light")] int Light,
        [property: JsonPropertyName("fanSpeed")] int FanSpeed,
        [property: JsonPropertyName("lightSchedule")] LightSchedule LightSchedule,
        [property: JsonPropertyName("bme")] Bme Bme,
        [property: JsonPropertyName("bmeRetained")] IReadOnlyList<Bme> BmeRetained
    );
}