using Microsoft.Extensions.Time.Testing;

namespace MoonDiary.Tests.Support;

public static class TestTime
{
    public static readonly TimeSpan Minsk = TimeSpan.FromHours(3);

    public static FakeTimeProvider At(int year, int month, int day, int hour, int minute, TimeSpan offset)
    {
        var time = new FakeTimeProvider();
        var zone = TimeZoneInfo.CreateCustomTimeZone("Test", offset, "Test", "Test");
        time.SetLocalTimeZone(zone);

        var local = new DateTimeOffset(year, month, day, hour, minute, 0, offset);
        time.SetUtcNow(local.ToUniversalTime());

        return time;
    }

    public static FakeTimeProvider At(int year, int month, int day, int hour, int minute) =>
        At(year, month, day, hour, minute, Minsk);
}
