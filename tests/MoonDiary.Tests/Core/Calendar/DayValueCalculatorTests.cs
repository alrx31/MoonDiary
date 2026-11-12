using MoonDiary.Core.Calendar;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Core.Calendar;

public class DayValueCalculatorTests
{
    private static readonly DateOnly Day = Dates.D(2026, 11, 9);

    [Fact]
    public void Calculate_EveningAndCheckins_UsesEvening()
    {
        var evening = EntryBuilder.Evening(Day).WithMood(4, 5).Build();
        var checkins = new[] { EntryBuilder.Checkin(Day).WithMood(1, 1).Build() };

        Assert.Equal(new DayMood(4, 5, true), DayValueCalculator.Calculate(evening, checkins));
    }

    [Fact]
    public void Calculate_TwoCheckins_RoundsHalfUp()
    {
        var checkins = new[]
        {
            EntryBuilder.Checkin(Day).WithMood(2, 3).Build(),
            EntryBuilder.Checkin(Day).WithMood(3, 4).Build()
        };

        Assert.Equal(new DayMood(3, 4, false), DayValueCalculator.Calculate(null, checkins));
    }

    [Fact]
    public void Calculate_ThreeCheckins_RoundsToNearest()
    {
        var checkins = new[]
        {
            EntryBuilder.Checkin(Day).WithMood(1, 1).Build(),
            EntryBuilder.Checkin(Day).WithMood(2, 2).Build(),
            EntryBuilder.Checkin(Day).WithMood(2, 2).Build()
        };

        Assert.Equal(new DayMood(2, 2, false), DayValueCalculator.Calculate(null, checkins));
    }

    [Fact]
    public void Calculate_NoEntries_ReturnsNull()
    {
        Assert.Null(DayValueCalculator.Calculate(null, []));
    }
}
