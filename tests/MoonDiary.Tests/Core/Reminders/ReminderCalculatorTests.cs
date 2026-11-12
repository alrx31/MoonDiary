using MoonDiary.Core.Domain;
using MoonDiary.Core.Reminders;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Core.Reminders;

public class ReminderCalculatorTests
{
    private static readonly DateOnly Today = Dates.D(2026, 11, 10);

    [Fact]
    public void GetNext_BeforeReminderWithoutEntry_IsToday()
    {
        Assert.Equal(Local(10, 18, 0), ReminderCalculator.GetNext(Local(10, 12, 0), Today, false, new AppSettings(), []));
    }

    [Fact]
    public void GetNext_AfterReminderOrWithEntry_IsTomorrow()
    {
        var settings = new AppSettings();

        Assert.Equal(Local(11, 18, 0), ReminderCalculator.GetNext(Local(10, 19, 0), Today, false, settings, []));
        Assert.Equal(Local(11, 18, 0), ReminderCalculator.GetNext(Local(10, 12, 0), Today, true, settings, []));
    }

    [Fact]
    public void GetNext_Disabled_IsNull()
    {
        var settings = new AppSettings { ReminderEnabled = false };

        Assert.Null(ReminderCalculator.GetNext(Local(10, 12, 0), Today, false, settings, []));
    }

    [Fact]
    public void GetReminderOffset_ThirteenEntriesOrAdaptiveOff_UsesFixedTime()
    {
        var off = new AppSettings { AdaptiveReminder = false };

        Assert.Equal(TimeSpan.FromHours(18), ReminderCalculator.GetReminderOffset(new AppSettings(), Evenings(13, 21, 15)));
        Assert.Equal(TimeSpan.FromHours(18), ReminderCalculator.GetReminderOffset(off, Evenings(14, 21, 15)));
    }

    [Fact]
    public void GetReminderOffset_FourteenEntries_IsMedianMinusTenMinutes()
    {
        Assert.Equal(new TimeSpan(21, 5, 0), ReminderCalculator.GetReminderOffset(new AppSettings(), Evenings(14, 21, 15)));
    }

    [Fact]
    public void GetReminderOffset_AroundMidnight_ShiftsNightTimes()
    {
        var mixed = Evenings(7, 23, 50).Concat(Evenings(7, 0, 10)).ToList();

        Assert.Equal(new TimeSpan(23, 50, 0), ReminderCalculator.GetReminderOffset(new AppSettings(), mixed));
    }

    [Fact]
    public void GetNext_MedianAfterMidnight_FallsOnNextDate()
    {
        var next = ReminderCalculator.GetNext(Local(10, 12, 0), Today, false, new AppSettings(), Evenings(14, 0, 20));

        Assert.Equal(Local(11, 0, 10), next);
    }

    [Fact]
    public async Task Service_BackfilledEntries_AreIgnored()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);

        for (var offset = 1; offset <= 14; offset++)
        {
            await app.EveningAsync(Today.AddDays(-offset), 3, 3);
        }

        Assert.Equal(Local(10, 18, 0), await app.ReminderService.GetNextReminderAsync());
    }

    private static DateTimeOffset Local(int day, int hour, int minute) =>
        new(2026, 11, day, hour, minute, 0, TestTime.Minsk);

    private static List<Entry> Evenings(int count, int hour, int minute) =>
        Enumerable.Range(1, count)
            .Select(index =>
            {
                var day = Today.AddDays(-index);
                var date = hour < 4 ? day.AddDays(1) : day;
                var utc = new DateTimeOffset(date.ToDateTime(new TimeOnly(hour, minute)), TestTime.Minsk).UtcDateTime;

                return EntryBuilder.Evening(day).CreatedAt(utc).Build();
            })
            .ToList();
}
