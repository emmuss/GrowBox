namespace GrowBox.Services;

public sealed class GrowBoxRepositoryFactory(ILogger<GrowBoxRepository> logger)
{
    public GrowBoxRepository Create(Abstractions.Model.GrowBox growBox) =>
        new (logger, growBox);
}