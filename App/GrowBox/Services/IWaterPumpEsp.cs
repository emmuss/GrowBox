using GrowBox.Abstractions.Model.EspApi;
using Refit;

namespace GrowBox.Services;

public interface IWaterPumpsEsp
{
    [Get("/get")]
    Task<WaterPumpsEspRoot> Get();
    [Post("/pump/set")]
    Task<WaterPumpsEspRoot> Set(WaterPump pump);
    [Post("/pump/set/all")]
    Task<WaterPumpsEspRoot> SetAll(WaterPump pump);
    [Post("/pump/test")]
    Task<WaterPumpsEspRoot> Test(WaterPump pump);
    [Post("/pump/test/all")]
    Task<WaterPumpsEspRoot> TestAll(WaterPump pump);
}