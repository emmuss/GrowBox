namespace GrowBox.Abstractions.Model.EspApi;

public sealed record LightScheduleReq(bool SunScheduleEnabled, int Sunrise, int SunDuration, int SunTargetLight);