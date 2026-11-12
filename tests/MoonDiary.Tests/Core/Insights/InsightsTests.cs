using MoonDiary.Core.Domain;
using MoonDiary.Core.Insights;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Insights;

public class InsightsTests
{
    private static readonly DateOnly Today = Dates.D(2026, 11, 10);

    [Fact]
    public void Streak_CountsConsecutiveDaysUpToToday()
    {
        var days = new HashSet<DateOnly> { Today, Today.AddDays(-1), Today.AddDays(-2), Today.AddDays(-4) };

        Assert.Equal(3, StreakCalculator.Calculate(days, Today));
    }

    [Fact]
    public void Streak_WithoutTodayEntry_CountsFromYesterday()
    {
        var days = new HashSet<DateOnly> { Today.AddDays(-1), Today.AddDays(-2) };

        Assert.Equal(2, StreakCalculator.Calculate(days, Today));
        Assert.Equal(0, StreakCalculator.Calculate(new HashSet<DateOnly> { Today.AddDays(-2) }, Today));
    }

    [Fact]
    public async Task Dynamics_ReturnsTwentyEightPointsWithCheckinAverage()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);
        await app.CheckinService.CreateAsync(2, 2, null);
        await app.CheckinService.CreateAsync(4, 4, null);
        await app.EveningAsync(Today.AddDays(-1), 5, 1);

        var points = await app.InsightsService.GetDynamicsAsync();

        Assert.Equal(28, points.Count);
        Assert.Equal(Today, points[^1].Date);
        Assert.Equal(3, points[^1].Mood!.Energy);
        Assert.Equal(5, points[^2].Mood!.Energy);
        Assert.Null(points[0].Mood);
        Assert.Equal(1, await app.InsightsService.GetStreakAsync());
    }

    [Fact]
    public async Task MonthAchievements_SkipEmptyAndGoFromNewest()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        await app.EveningAsync(Today.AddDays(-3), 3, 3);

        foreach (var (offset, text) in new[] { (-2, "первое"), (0, "второе") })
        {
            var state = await app.EntryService.OpenWizardAsync(Today.AddDays(offset));
            state.SetEnergy(3);
            state.SetPositivity(3);
            state.SetText(CardType.Achievement, text);
            await app.EntryService.SaveEveningAsync(state);
        }

        var achievements = await app.InsightsService.GetMonthAchievementsAsync(2026, 11);

        Assert.Equal(["второе", "первое"], achievements.Select(item => item.Text));
    }
}
