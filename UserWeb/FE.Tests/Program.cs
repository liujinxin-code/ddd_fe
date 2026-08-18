// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;

Console.WriteLine($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"转换上海时区：{TimeZoneHelper.NowInZone("Asia/Shanghai"):yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"转换东京时区：{TimeZoneHelper.NowInZone("Asia/Tokyo"):yyyy-MM-dd HH:mm:ss}");
Console.WriteLine("Hello, World!");
Console.ReadLine();



public static class TimeZoneHelper
{
    // 缓存时区对象，避免反复 FindSystemTimeZoneById
    private static readonly ConcurrentDictionary<string, TimeZoneInfo> Cache = new();

    public static DateTimeOffset UtcToZone(DateTimeOffset utc, string tzId)
    {
        var zone = GetZone(tzId);
        return TimeZoneInfo.ConvertTime(utc, zone);
    }

    public static DateTimeOffset NowInZone(string tzId)
        => UtcToZone(DateTimeOffset.Now, tzId);

    private static TimeZoneInfo GetZone(string tzId)
    {
        return Cache.GetOrAdd(tzId, TimeZoneInfo.FindSystemTimeZoneById);
    }
}