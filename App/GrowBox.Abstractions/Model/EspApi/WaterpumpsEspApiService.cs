using System.Net.Http.Json;

namespace GrowBox.Abstractions.Model.EspApi;

public sealed class WaterpumpsEspApiService(HttpClient http, string waterpumpsUrl)
{
    public Task<WaterPumpsEspRoot?> Get(CancellationToken cancellationToken = default)
    {
        var url = Path.Combine(waterpumpsUrl, "get");
        return http.GetFromJsonAsync<WaterPumpsEspRoot>(url, cancellationToken: cancellationToken);
    }

    public async Task<WaterPumpsEspRoot?> SetPump(WaterPump pump, CancellationToken cancellationToken = default)
    {
        var url = Path.Combine(waterpumpsUrl, "pump/set");
        var resp = await http.PostAsJsonAsync(url, pump, cancellationToken: cancellationToken);
        return await resp.Content.ReadFromJsonAsync<WaterPumpsEspRoot>(cancellationToken: cancellationToken);
    }
}