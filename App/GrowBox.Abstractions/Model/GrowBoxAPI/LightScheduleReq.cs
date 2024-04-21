namespace GrowBox.Abstractions.Model.GrowBoxAPI;

public sealed record LightScheduleReq(bool SunScheduleEnabled, int Sunrise, int SunDuration, int SunTargetLight);