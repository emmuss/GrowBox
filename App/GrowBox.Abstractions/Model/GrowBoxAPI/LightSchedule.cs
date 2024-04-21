using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.GrowBoxAPI
{
    public record LightSchedule(
        [property: JsonPropertyName("sunScheduleEnabled")] bool SunScheduleEnabled,
        [property: JsonPropertyName("sunrise")] int Sunrise,
        [property: JsonPropertyName("sunDuration")] int SunDuration,
        [property: JsonPropertyName("sunTargetLight")] int SunTargetLight
    );
}