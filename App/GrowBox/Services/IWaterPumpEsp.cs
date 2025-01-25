using GrowBox.Abstractions.Model.EspApi;
using Refit;

namespace GrowBox.Services;

public interface IWaterPumpsEsp
{
    [Get("/get")]
    Task<WaterPumpsEspRoot> Get();
    [Post("/pump/set")]
    Task<WaterPumpsEspRoot> Set(WaterPump pump);
    [Post("/pumps/set")]
    Task<WaterPumpsEspRoot> Set(WaterPumps pumps);
    [Post("/pump/test")]
    Task<WaterPumpsEspRoot> Test(WaterPump pump);
}