namespace GrowBox.Services;

public sealed class GrowBoxRepositoryFactory(HttpClient http, ILogger<GrowBoxRepository> logger)
{
    public GrowBoxRepository Create(Abstractions.Model.GrowBox growBox) =>
        new GrowBoxRepository(http, logger, growBox);
}