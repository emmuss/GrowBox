using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.EspApi
{
    public enum VPDControlPlantStage
    {
        Propagation = 0,
        EarlyGrow = 1,
        LateGrow = 2,
        EarlyFlower = 3,
        LateFlower = 4,
    }
    public record GrowBoxEspRoot(
        [property: JsonPropertyName("me")] string Me,
        [property: JsonPropertyName("fanSpeed")] int FanSpeed,
        [property: JsonPropertyName("light")] int Light,
        [property: JsonPropertyName("temperature")] double Temperature,
        [property: JsonPropertyName("humidity")] double Humidity,
        [property: JsonPropertyName("pressure")] double Pressure,
        [property: JsonPropertyName("dewPoint")] double DewPoint,
        [property: JsonPropertyName("heatIndex")] double HeatIndex,
        [property: JsonPropertyName("timestamp")] int Timestamp,
        [property: JsonPropertyName("vpd")] double VPD,
        [property: JsonPropertyName("vpdControlPlantStage")] VPDControlPlantStage VPDControlPlantStage,
        [property: JsonPropertyName("vpdControlPlantStageTimestamp")] int VPDControlPlantStageTimestamp,
        [property: JsonPropertyName("lightSchedule")] LightSchedule LightSchedule
    );
}