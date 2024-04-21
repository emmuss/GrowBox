namespace GrowBox.Abstractions;

public record Configuration(string GrowBoxUrl, string MotionUrl, TimeSpan UpdateTimeout)
{
    public static readonly Configuration DefaultConfig = new(
        "http://192.168.178.183/",
        "http://192.168.178.200:8080/",
        TimeSpan.FromSeconds(5)
    );
};