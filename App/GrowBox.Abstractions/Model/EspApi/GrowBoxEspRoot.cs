using System.Text.Json.Serialization;

namespace GrowBox.Abstractions.Model.EspApi
{
    public enum VPDControlPlantStage
    {
        Off = 0,
        Propagation = 1,
        EarlyGrow = 2,
        LateGrow = 3,
        EarlyFlower = 4,
        LateFlower = 5,
    }

    public record VPDControlMinMax(double Min, double Low, double High, double Max)
    {
        public double GetPercentage(double value) => (value - Min) / (Max - Min);
        public double GetValue(double percentage) => Min + (Max - Min) * percentage;
    }

    public record VPDControlConstraint(
        VPDControlPlantStage PlantStage,
        VPDControlMinMax VPD,
        VPDControlMinMax Humidity,
        VPDControlMinMax Temperature
    )
    {

        /*
             			            VPD		    TMP	    HUM
            Propagation (Keimung)   0,1 – 0,3	22 – 23	75 – 85
            Frühe Wachstumsphase	0,4 – 0,8	24 – 34	75 – 80
            Späte Wachstumsphase	0,7 – 1,0	24 – 34	55 – 75
            Frühe Blütephase	    0,8 – 1,2	24 – 34	50 – 75
            Späte Blütephase	    1,2 – 1,6	24 – 34	40 – 65
         */
        public static VPDControlConstraint[] Default = [
            new ( VPDControlPlantStage.Off, new VPDControlMinMax(0, 0, 5, 5 ), new VPDControlMinMax(0, 0, 200, 200 ), new VPDControlMinMax(0, 0, 40, 40 ) ),
            new ( VPDControlPlantStage.Propagation, new VPDControlMinMax(0, 0.1, 0.3, 1.8), new VPDControlMinMax(0, 75, 85, 100), new VPDControlMinMax(18, 22, 23, 36)),
            new ( VPDControlPlantStage.EarlyGrow,   new VPDControlMinMax(0, 0.4, 0.8, 1.8), new VPDControlMinMax(0, 75, 80, 100), new VPDControlMinMax(18, 22, 34, 36)),
            new ( VPDControlPlantStage.LateGrow,    new VPDControlMinMax(0, 0.7, 1.0, 1.8), new VPDControlMinMax(0, 55, 75, 100), new VPDControlMinMax(18, 22, 34, 36)),
            new ( VPDControlPlantStage.EarlyFlower, new VPDControlMinMax(0, 0.8, 1.2, 1.8), new VPDControlMinMax(0, 50, 75, 100), new VPDControlMinMax(18, 22, 34, 36)),
            new ( VPDControlPlantStage.LateFlower,  new VPDControlMinMax(0, 1.2, 1.6, 1.8), new VPDControlMinMax(0, 40, 65, 100), new VPDControlMinMax(18, 22, 34, 36))
        ];
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