using MoonDiary.Core.Entries;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Letters;

public class LetterServiceTests
{
    [Fact]
    public async Task Create_TodayOrInvalidText_IsRejected()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);

        await Assert.ThrowsAsync<DomainException>(() => app.LetterService.CreateAsync("привет", Dates.D(2026, 11, 10)));
        await Assert.ThrowsAsync<DomainException>(() => app.LetterService.CreateAsync("  ", Dates.D(2026, 11, 11)));
        await Assert.ThrowsAsync<DomainException>(
            () => app.LetterService.CreateAsync(new string('а', 5001), Dates.D(2026, 11, 11)));

        Assert.Equal(Dates.D(2026, 11, 11), (await app.LetterService.CreateAsync("привет", Dates.D(2026, 11, 11))).OpenDate);
    }

    [Fact]
    public void MinOpenDate_AtNight_CountsFromDayKey()
    {
        using var app = TestApp.At(2026, 11, 11, 2, 0);

        Assert.Equal(Dates.D(2026, 11, 11), app.LetterService.MinOpenDate);
    }

    [Fact]
    public async Task Letter_OpensAtFourInTheMorning()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);
        var letter = await app.LetterService.CreateAsync("ты справился", Dates.D(2026, 11, 11));

        app.SetLocal(2026, 11, 11, 3, 59);
        Assert.Empty(await app.LetterService.OpenDueAsync());
        await Assert.ThrowsAsync<DomainException>(() => app.LetterService.ReadAsync(letter.Id));
        Assert.Null((await app.LetterService.GetListAsync())[0].Preview);

        app.SetLocal(2026, 11, 11, 4, 0);
        Assert.Single(await app.LetterService.OpenDueAsync());
        Assert.Equal("ты справился", (await app.LetterService.ReadAsync(letter.Id)).Text);
        Assert.Equal("ты справился", (await app.LetterService.GetListAsync())[0].Preview);
    }

    [Fact]
    public async Task GetList_ClosedFirstByDateThenOpenFromNewest()
    {
        using var app = TestApp.At(2026, 11, 1, 12, 0);
        await app.LetterService.CreateAsync("а", Dates.D(2026, 11, 5));
        await app.LetterService.CreateAsync("б", Dates.D(2026, 11, 8));
        await app.LetterService.CreateAsync("в", Dates.D(2026, 12, 20));
        await app.LetterService.CreateAsync("г", Dates.D(2026, 12, 1));
        app.SetLocal(2026, 11, 10, 12, 0);

        var list = await app.LetterService.GetListAsync();

        Assert.Equal(
            [Dates.D(2026, 12, 1), Dates.D(2026, 12, 20), Dates.D(2026, 11, 8), Dates.D(2026, 11, 5)],
            list.Select(item => item.OpenDate));
    }

    [Fact]
    public async Task PendingNotifications_AreAtTenAndStopAfterMark()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);
        var letter = await app.LetterService.CreateAsync("привет", Dates.D(2026, 11, 12));

        var pending = Assert.Single(await app.LetterService.GetPendingNotificationsAsync());
        Assert.Equal(new DateTimeOffset(2026, 11, 12, 10, 0, 0, TestTime.Minsk), pending.NotifyAtLocal);

        await app.LetterService.MarkNotifiedAsync(letter.Id);
        Assert.Empty(await app.LetterService.GetPendingNotificationsAsync());

        await app.LetterService.DeleteAsync(letter.Id);
        Assert.Empty(await app.LetterService.GetListAsync());
    }
}
