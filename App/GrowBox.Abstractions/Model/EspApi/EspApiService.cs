using System.Net.Http.Json;

namespace GrowBox.Abstractions.Model.EspApi;

public sealed class EspApiService(HttpClient http, string growBoxUrl)
{
    public Task<EspRoot?> Get(CancellationToken cancellationToken = default)
    {
        var url = Path.Combine(growBoxUrl, "get");
        return http.GetFromJsonAsync<EspRoot>(url, cancellationToken: cancellationToken);
    }

    public async Task<EspRoot?> FanSet(int speed, CancellationToken cancellationToken = default)
    {
        var url = Path.Combine(growBoxUrl, "fan/set");
        var resp = await http.PostAsJsonAsync(url, new { fanSpeed = speed }, cancellationToken: cancellationToken);
        return await resp.Content.ReadFromJsonAsync<EspRoot>(cancellationToken: cancellationToken);
    }
    public async Task<EspRoot?> LightSet(int light, CancellationToken cancellationToken = default)
    {
        var url = Path.Combine(growBoxUrl, "light/set");
        var resp = await http.PostAsJsonAsync(url, new { light=light }, cancellationToken: cancellationToken);
        return await resp.Content.ReadFromJsonAsync<EspRoot>(cancellationToken: cancellationToken);
    }
    public async Task<EspRoot?> LightScheduleSet(LightScheduleReq request, CancellationToken cancellationToken = default)
    {
        var url = Path.Combine(growBoxUrl, "light/schedule/set");
        var resp = await http.PostAsJsonAsync(url, new {
            sunScheduleEnabled=request.SunScheduleEnabled,
            sunrise = request.Sunrise,
            sunDuration=request.SunDuration,
            sunriseSetDuration=480,
            sunTargetLight = request.SunTargetLight
        }, cancellationToken: cancellationToken);
        return await resp.Content.ReadFromJsonAsync<EspRoot>(cancellationToken: cancellationToken);
    }
}