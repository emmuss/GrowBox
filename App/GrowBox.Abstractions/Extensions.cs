namespace GrowBox.Abstractions;

public static class Extensions
{
    public static DateTime ToDateFromUnixSeconds(this int timestamp)
        => DateTime.UnixEpoch.AddSeconds(timestamp);
    public static int ToUnixSecondsFromDate(this DateTime dateTime)
        => (int)(dateTime - DateTime.UnixEpoch).TotalSeconds;
}