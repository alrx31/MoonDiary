using MoonDiary.Core.Domain;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Calendar;

public class CalendarServiceTests
{
    [Fact]
    public async Task GetMonth_CurrentMonth_MarksTodayAndFuture()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);

        var month = await app.CalendarService.GetMonthAsync(2026, 11);

        Assert.Equal(30, month.Days.Count);
        Assert.True(month.Days[9].IsToday);
        Assert.True(month.Days[10].IsFuture);
        Assert.False(month.CanGoNext);
        Assert.True((await app.CalendarService.GetMonthAsync(2026, 10)).CanGoNext);
    }

    [Fact]
    public async Task GetMonth_AtTwoAtNight_TodayIsPreviousDate()
    {
        using var app = TestApp.At(2026, 11, 11, 2, 0);

        var month = await app.CalendarService.GetMonthAsync(2026, 11);

        Assert.True(month.Days[9].IsToday);
        Assert.False(month.Days[10].IsToday);
    }

    [Fact]
    public async Task GetMonth_DayWithCheckinsOnly_UsesAverage()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);
        await app.CheckinService.CreateAsync(2, 3, null);
        await app.CheckinService.CreateAsync(3, 4, null);

        var day = (await app.CalendarService.GetMonthAsync(2026, 11)).Days[9];

        Assert.False(day.HasEvening);
        Assert.Equal(2, day.CheckinCount);
        Assert.Equal(3, day.Mood!.Energy);
        Assert.Equal(4, day.Mood.Positivity);
    }

    [Fact]
    public async Task GetMonth_ShortText_IsAchievementOrFirstDoneTag()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        await app.EveningAsync(Dates.D(2026, 11, 9), 3, 3, (CardType.Feelings, "спокойно"), (CardType.Done, "спорт"));
        var state = await app.EntryService.OpenWizardAsync(Dates.D(2026, 11, 10));
        state.SetEnergy(4);
        state.SetPositivity(4);
        state.SetText(CardType.Achievement, "  дописал отчёт\nи отправил ");
        await app.EntryService.SaveEveningAsync(state);

        var month = await app.CalendarService.GetMonthAsync(2026, 11);

        Assert.Equal("спорт", month.Days[8].ShortText);
        Assert.Equal("дописал отчёт", month.Days[9].ShortText);
    }

    [Fact]
    public async Task GetFeed_CurrentMonth_StartsFromTodayWithoutFuture()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);

        var feed = await app.CalendarService.GetFeedAsync(2026, 11);

        Assert.Equal(10, feed.Count);
        Assert.Equal(Dates.D(2026, 11, 10), feed[0].Date);
    }

    [Fact]
    public async Task GetDay_EntryWithTagsAndCheckins_ReturnsDetails()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        await app.CheckinService.CreateAsync(2, 2, "стресс");
        await app.EveningAsync(
            Dates.D(2026, 11, 10), 4, 3, (CardType.Feelings, "спокойно"), (CardType.Done, "спорт"), (CardType.Done, "работа"));

        var day = await app.CalendarService.GetDayAsync(Dates.D(2026, 11, 10));

        Assert.NotNull(day.Evening);
        Assert.Single(day.Checkins);
        Assert.True(day.Mood!.FromEvening);
        Assert.Equal(["работа", "спорт", "спокойно"], day.EveningTags.Select(tag => tag.Label));
        Assert.True(day.IsToday);
        Assert.True((await app.CalendarService.GetDayAsync(Dates.D(2026, 11, 11))).IsFuture);
    }
}
