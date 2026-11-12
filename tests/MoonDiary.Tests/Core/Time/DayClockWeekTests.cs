using MoonDiary.Core.Time;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Time;

public class DayClockWeekTests
{
    [Fact]
    public void GetWeekStart_Monday_ReturnsSameDay()
    {
        Assert.Equal(new DateOnly(2026, 11, 9), DayClock.GetWeekStart(new DateOnly(2026, 11, 9)));
    }

    [Fact]
    public void GetWeekStart_Tuesday_ReturnsMonday()
    {
        Assert.Equal(new DateOnly(2026, 11, 9), DayClock.GetWeekStart(new DateOnly(2026, 11, 10)));
    }

    [Fact]
    public void GetWeekStart_Sunday_ReturnsMonday()
    {
        Assert.Equal(new DateOnly(2026, 11, 9), DayClock.GetWeekStart(new DateOnly(2026, 11, 15)));
    }

    [Fact]
    public void LastCompletedWeekStart_MondayAt0400_ReturnsPreviousWeekStart()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 16, 4, 0));

        Assert.Equal(new DateOnly(2026, 11, 9), clock.LastCompletedWeekStart);
    }

    [Fact]
    public void LastCompletedWeekStart_MondayAt0300_ReturnsWeekBeforePrevious()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 16, 3, 0));

        Assert.Equal(new DateOnly(2026, 11, 2), clock.LastCompletedWeekStart);
    }

    [Fact]
    public void CurrentWeekStart_MondayAt0400_ReturnsSameDay()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 16, 4, 0));

        Assert.Equal(new DateOnly(2026, 11, 16), clock.CurrentWeekStart);
    }

    [Fact]
    public void ToDayKey_At0030UtcInPlus3_ReturnsPreviousDay()
    {
        var utc = new DateTime(2026, 11, 10, 0, 30, 0, DateTimeKind.Utc);

        Assert.Equal(new DateOnly(2026, 11, 9), DayClock.ToDayKey(utc, 180));
    }

    [Fact]
    public void ToLocal_At0030UtcInPlus3_ReturnsLocalTimeWithOffset()
    {
        var utc = new DateTime(2026, 11, 10, 0, 30, 0, DateTimeKind.Utc);

        var local = DayClock.ToLocal(utc, 180);

        Assert.Equal(new DateTime(2026, 11, 10, 3, 30, 0), local.DateTime);
        Assert.Equal(TimeSpan.FromHours(3), local.Offset);
    }

    [Fact]
    public void GetDayStartLocal_ForDay_Returns0400WithLocalOffset()
    {
        var clock = new DayClock(TestTime.At(2026, 11, 10, 12, 0));

        var start = clock.GetDayStartLocal(new DateOnly(2026, 11, 10));

        Assert.Equal(new DateTime(2026, 11, 10, 4, 0, 0), start.DateTime);
        Assert.Equal(TimeSpan.FromHours(3), start.Offset);
    }
}
