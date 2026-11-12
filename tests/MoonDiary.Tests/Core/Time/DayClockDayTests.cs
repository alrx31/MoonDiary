using MoonDiary.Core.Time;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Time;

public class DayClockDayTests
{
    [Fact]
    public void LocalNow_AtMinskTime_ReturnsLocalTimeWithOffset()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 3, 59));

        Assert.Equal(new DateTime(2026, 11, 10, 3, 59, 0), clock.LocalNow.DateTime);
        Assert.Equal(TimeSpan.FromHours(3), clock.LocalNow.Offset);
    }

    [Fact]
    public void UtcNow_AtMinskTime_ReturnsUtcKind()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 3, 59));

        Assert.Equal(new DateTime(2026, 11, 10, 0, 59, 0, DateTimeKind.Utc), clock.UtcNow);
    }

    [Fact]
    public void LocalOffsetMinutes_AtMinskTime_Returns180()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 12, 0));

        Assert.Equal(180, clock.LocalOffsetMinutes);
    }

    [Fact]
    public void CurrentDayKey_At0359_ReturnsPreviousDay()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 3, 59));

        Assert.Equal(new DateOnly(2026, 11, 9), clock.CurrentDayKey);
    }

    [Fact]
    public void IsInFillWindow_At0359_ReturnsTrue()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 3, 59));

        Assert.True(clock.IsInFillWindow);
    }

    [Fact]
    public void CurrentDayKey_At0400_ReturnsSameDay()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 4, 0));

        Assert.Equal(new DateOnly(2026, 11, 10), clock.CurrentDayKey);
    }

    [Fact]
    public void IsInFillWindow_At0400_ReturnsFalse()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 4, 0));

        Assert.False(clock.IsInFillWindow);
    }

    [Fact]
    public void IsInFillWindow_At1759_ReturnsFalse()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 17, 59));

        Assert.False(clock.IsInFillWindow);
    }

    [Fact]
    public void IsInFillWindow_At1800_ReturnsTrue()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 18, 0));

        Assert.True(clock.IsInFillWindow);
    }

    [Fact]
    public void IsInFillWindow_At2359_ReturnsTrue()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 23, 59));

        Assert.True(clock.IsInFillWindow);
    }

    [Fact]
    public void CurrentDayKey_At2359_ReturnsSameDay()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 23, 59));

        Assert.Equal(new DateOnly(2026, 11, 10), clock.CurrentDayKey);
    }

    [Fact]
    public void CurrentDayKey_AtMidnightOnNewYear_ReturnsPreviousYearDay()
    {
        var clock = new DayClock(TestTime.At(2027, 1, 1, 0, 0));

        Assert.Equal(new DateOnly(2026, 12, 31), clock.CurrentDayKey);
    }

    [Fact]
    public void CurrentDayKey_At0030UtcInPlus3_ReturnsPreviousDay()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 3, 30, TimeSpan.FromHours(3)));

        Assert.Equal(new DateOnly(2026, 11, 9), clock.CurrentDayKey);
    }

    [Fact]
    public void IsInFillWindow_At0030UtcInPlus3_ReturnsTrue()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 3, 30, TimeSpan.FromHours(3)));

        Assert.True(clock.IsInFillWindow);
    }

    [Fact]
    public void CurrentDayKey_At0030UtcInPlus4_ReturnsSameDay()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 4, 30, TimeSpan.FromHours(4)));

        Assert.Equal(new DateOnly(2026, 11, 10), clock.CurrentDayKey);
    }

    [Fact]
    public void IsInFillWindow_At0030UtcInPlus4_ReturnsFalse()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 4, 30, TimeSpan.FromHours(4)));

        Assert.False(clock.IsInFillWindow);
    }

    [Fact]
    public void GetDayKey_At0359_ReturnsPreviousDay()
    {
        var local = new DateTimeOffset(2026, 11, 10, 3, 59, 0, TimeSpan.FromHours(3));

        Assert.Equal(new DateOnly(2026, 11, 9), DayClock.GetDayKey(local));
    }

    [Fact]
    public void GetDayKey_At1700_ReturnsSameDay()
    {
        var local = new DateTimeOffset(2026, 11, 10, 17, 0, 0, TimeSpan.FromHours(3));

        Assert.Equal(new DateOnly(2026, 11, 10), DayClock.GetDayKey(local));
    }

    [Fact]
    public void IsInFillWindowAt_At1759_ReturnsFalse()
    {
        var local = new DateTimeOffset(2026, 11, 10, 17, 59, 0, TimeSpan.FromHours(3));

        Assert.False(DayClock.IsInFillWindowAt(local));
    }

    [Fact]
    public void IsInFillWindowAt_At1800_ReturnsTrue()
    {
        var local = new DateTimeOffset(2026, 11, 10, 18, 0, 0, TimeSpan.FromHours(3));

        Assert.True(DayClock.IsInFillWindowAt(local));
    }
}
