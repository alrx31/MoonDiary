using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Wizard;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Entries;

public class EntryServiceTests
{
    private static readonly DateOnly Today = Dates.D(2026, 11, 10);

    [Fact]
    public async Task SaveEvening_MoodOnly_CreatesEntry()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);

        var entry = await app.EveningAsync(Today, 4, 4);

        Assert.Equal(EntryType.Evening, entry.Type);
        Assert.Null(entry.Done);
        Assert.False(entry.IsBackfilled);
        Assert.Equal(180, entry.TimezoneOffsetMinutes);
    }

    [Fact]
    public async Task SaveEvening_PastDay_IsBackfilled()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);

        var entry = await app.EveningAsync(Today.AddDays(-3), 3, 3);

        Assert.True(entry.IsBackfilled);
    }

    [Fact]
    public async Task OpenWizard_FutureDay_Throws()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);

        await Assert.ThrowsAsync<DomainException>(() => app.EntryService.OpenWizardAsync(Today.AddDays(1)));
    }

    [Fact]
    public async Task SaveEvening_SecondTime_EditsSameEntry()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        var first = await app.EveningAsync(Today, 2, 2);
        app.Time.Advance(TimeSpan.FromHours(1));

        var state = await app.EntryService.OpenWizardAsync(Today);
        state.SetEnergy(5);
        var second = await app.EntryService.SaveEveningAsync(state);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.CreatedAtUtc, second.CreatedAtUtc);
        Assert.NotEqual(first.UpdatedAtUtc, second.UpdatedAtUtc);
        Assert.Equal(5, second.Energy);
        Assert.Equal(1, app.Database.Scalar<int>("SELECT COUNT(*) FROM entries"));
    }

    [Fact]
    public async Task SaveEvening_ErasedLabel_DropsLink()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        var sport = await app.TagAsync(CardType.Done, "спорт");
        var state = await app.EntryService.OpenWizardAsync(Today);
        state.SetEnergy(3);
        state.SetPositivity(3);
        state.ToggleTag(CardType.Done, sport);
        state.SetText(CardType.Done, "ничего");

        var entry = await app.EntryService.SaveEveningAsync(state);

        Assert.Empty(await app.Entries.GetEntryTagsAsync([entry.Id]));
        Assert.Equal(0, (await app.Tags.GetByIdAsync(sport.Id))!.UseCount);
    }

    [Fact]
    public async Task SaveEvening_SameTagTwice_IncrementsUseCountOnce()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        var sport = await app.TagAsync(CardType.Done, "спорт");

        await app.EveningAsync(Today, 3, 3, (CardType.Done, "спорт"));
        await app.EveningAsync(Today, 4, 3, (CardType.Done, "спорт"));

        Assert.Equal(1, (await app.Tags.GetByIdAsync(sport.Id))!.UseCount);
    }

    [Fact]
    public async Task SaveEvening_RemovesDraftOfThatDay()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        var draft = new EveningWizardState(Today);
        draft.SetEnergy(2);
        await app.EntryService.SaveDraftAsync(draft);

        await app.EveningAsync(Today, 3, 3);

        Assert.False(await app.EntryService.HasDraftAsync(Today));
    }

    [Fact]
    public async Task DeleteEvening_KeepsCheckins()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        await app.CheckinService.CreateAsync(3, 3, null);
        var entry = await app.EveningAsync(Today, 3, 3);

        await app.EntryService.DeleteEveningAsync(entry.Id);

        Assert.Null(await app.Entries.GetEveningAsync(Today));
        Assert.Single(await app.Entries.GetCheckinsAsync(Today));
    }

    [Fact]
    public async Task SaveDraft_EmptyState_IsNotSaved()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);

        Assert.False(await app.EntryService.SaveDraftAsync(new EveningWizardState(Today)));
        Assert.Empty(await app.EntryService.GetDraftsAsync());
    }

    [Fact]
    public async Task SaveDraft_OneScale_IsRestoredAndListed()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        var state = new EveningWizardState(Today);
        state.SetEnergy(2);

        Assert.True(await app.EntryService.SaveDraftAsync(state));
        state.SetPositivity(4);
        state.Next();
        state.SetText(CardType.Done, "работа");
        await app.EntryService.SaveDraftAsync(state);

        var restored = await app.EntryService.OpenDraftAsync(Today);
        var item = Assert.Single(await app.EntryService.GetDraftsAsync());

        Assert.Equal(WizardStep.Done, restored!.Step);
        Assert.Equal("работа", restored.GetText(CardType.Done));
        Assert.Equal(2, item.FilledCards);
        Assert.False(item.OnlyMood);
        Assert.Null((await app.EntryService.OpenWizardAsync(Today)).Energy);
    }

    [Fact]
    public async Task Checkin_AtTwoAtNight_BelongsToPreviousDay()
    {
        using var app = TestApp.At(2026, 11, 11, 2, 0);

        var first = await app.CheckinService.CreateAsync(3, 2, "  стресс ");
        await app.CheckinService.CreateAsync(4, 4, null);

        Assert.Equal(Today, first.DayKey);
        Assert.Equal("стресс", first.CheckinText);
        Assert.Equal(2, (await app.Entries.GetCheckinsAsync(Today)).Count);
    }

    [Fact]
    public async Task Checkin_UpdateAndDelete_Work()
    {
        using var app = TestApp.At(2026, 11, 10, 12, 0);
        var checkin = await app.CheckinService.CreateAsync(3, 2, null);

        var updated = await app.CheckinService.UpdateAsync(checkin.Id, 5, 5, "лучше");
        Assert.Equal(5, updated.Energy);
        Assert.Equal(checkin.DayKey, updated.DayKey);

        await app.CheckinService.DeleteAsync(checkin.Id);
        Assert.Empty(await app.Entries.GetCheckinsAsync(Today));
    }

    [Fact]
    public async Task Checkin_EveningEntryOrLongText_IsRejected()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        var evening = await app.EveningAsync(Today, 3, 3);

        await Assert.ThrowsAsync<DomainException>(() => app.CheckinService.DeleteAsync(evening.Id));
        await Assert.ThrowsAsync<DomainException>(
            () => app.CheckinService.CreateAsync(3, 3, new string('а', 501)));
    }
}
