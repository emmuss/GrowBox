namespace GrowBox.Abstractions;

public class ServerConfiguration
{
    public const string SECTION_KEY = nameof(Configuration);
    public string PgSqlConnectionString { get; set; } = string.Empty;
    public TimeSpan SensorReadingsRetention { get; set; } = TimeSpan.FromDays(365);
}