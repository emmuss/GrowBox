using GrowBox.Abstractions.Model.EspApi;
using Refit;

namespace GrowBox.Services;

public record FanSpeedReq(int FanSpeed);
public record LightReq(int Light);

public interface IGrowBoxEsp
{
    [Get("/get")]
    Task<GrowBoxEspRoot> Get();
    [Post("/fan/set")]
    Task<GrowBoxEspRoot> SetFanSpeed(FanSpeedReq fanSpeed);
    [Post("/light/set")]
    Task<GrowBoxEspRoot> SetLight(LightReq light);
    [Post("/light/schedule/set")]
    Task<GrowBoxEspRoot> SetLightSchedule(LightScheduleReq schedule);
}