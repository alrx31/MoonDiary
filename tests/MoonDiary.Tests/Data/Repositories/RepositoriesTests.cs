using Microsoft.Data.Sqlite;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Resources;
using MoonDiary.Data.Repositories;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Data.Repositories;

public sealed class RepositoriesTests : IDisposable
{
    private static readonly DateOnly Day = Dates.D(2026, 11, 9);

    private readonly TestDatabase database = new();

    public void Dispose() => database.Dispose();

    [Fact]
    public async Task Entry_SaveAndGet_RoundTripsAllFields()
    {
        var repository = new EntryRepository(database.Factory);
        var entry = EntryBuilder.Evening(Day)
            .WithMood(4, 2)
            .WithDone("работа")
            .WithAchievement("сдал задачу")
            .WithFeelings("спокойно")
            .Backfilled()
            .Build();

        await repository.SaveAsync(entry);
        var loaded = await repository.GetByIdAsync(entry.Id);

        Assert.NotNull(loaded);
        Assert.Equal(entry.Type, loaded.Type);
        Assert.Equal(entry.DayKey, loaded.DayKey);
        Assert.Equal(4, loaded.Energy);
        Assert.Equal(2, loaded.Positivity);
        Assert.Equal("работа", loaded.Done);
        Assert.Equal("сдал задачу", loaded.Achievement);
        Assert.Equal("спокойно", loaded.Feelings);
        Assert.Null(loaded.CheckinText);
        Assert.Equal(entry.CreatedAtUtc, loaded.CreatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, loaded.CreatedAtUtc.Kind);
        Assert.Equal(180, loaded.TimezoneOffsetMinutes);
        Assert.True(loaded.IsBackfilled);
    }

    [Fact]
    public async Task Entry_SaveTwice_UpdatesSingleRow()
    {
        var repository = new EntryRepository(database.Factory);
        var entry = EntryBuilder.Evening(Day).Build();

        await repository.SaveAsync(entry);
        entry.Energy = 5;
        await repository.SaveAsync(entry);

        Assert.Equal(1, database.Scalar<int>("SELECT COUNT(*) FROM entries"));
        Assert.Equal(5, (await repository.GetEveningAsync(Day))!.Energy);
    }

    [Fact]
    public async Task Entry_SaveWithZeroEnergy_Throws()
    {
        var repository = new EntryRepository(database.Factory);
        var entry = EntryBuilder.Evening(Day).Build();
        entry.Energy = 0;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.SaveAsync(entry));
    }

    [Fact]
    public async Task Entry_GetRange_IncludesBothBounds()
    {
        var repository = new EntryRepository(database.Factory);

        foreach (var offset in new[] { -1, 0, 1, 2 })
        {
            await repository.SaveAsync(EntryBuilder.Evening(Day.AddDays(offset)).Build());
        }

        var range = await repository.GetRangeAsync(Day, Day.AddDays(1));

        Assert.Equal([Day, Day.AddDays(1)], range.Select(entry => entry.DayKey));
    }

    [Fact]
    public async Task Entry_GetLatestEvenings_SkipsBackfilled()
    {
        var repository = new EntryRepository(database.Factory);
        await repository.SaveAsync(EntryBuilder.Evening(Day).Build());
        await repository.SaveAsync(EntryBuilder.Evening(Day.AddDays(1)).Backfilled().Build());

        var latest = await repository.GetLatestEveningsAsync(14, excludeBackfilled: true);

        Assert.Equal(Day, Assert.Single(latest).DayKey);
    }

    [Fact]
    public async Task EntryTags_Replace_KeepsLastSetAndCascadesOnDelete()
    {
        var entries = new EntryRepository(database.Factory);
        var tags = new TagRepository(database.Factory);
        var first = TagBuilder.For(CardType.Done, "работа").Build();
        var second = TagBuilder.For(CardType.Done, "спорт").WithOrder(1).Build();
        var entry = EntryBuilder.Evening(Day).Build();
        await tags.SaveAsync(first);
        await tags.SaveAsync(second);
        await entries.SaveAsync(entry);

        await entries.ReplaceEntryTagsAsync(entry.Id, [new EntryTag(entry.Id, first.Id, CardType.Done)]);
        await entries.ReplaceEntryTagsAsync(
            entry.Id,
            [new EntryTag(entry.Id, second.Id, CardType.Done), new EntryTag(entry.Id, second.Id, CardType.Feelings)]);

        var links = await entries.GetEntryTagsAsync([entry.Id]);
        Assert.Equal(2, links.Count);
        Assert.All(links, link => Assert.Equal(second.Id, link.TagId));

        await entries.DeleteAsync(entry.Id);
        Assert.Equal(0, database.Scalar<int>("SELECT COUNT(*) FROM entry_tags"));
    }

    [Fact]
    public async Task Tags_Seed_FillsOnceInFileOrder()
    {
        var repository = new TagRepository(database.Factory);

        Assert.True(await repository.SeedIfEmptyAsync(StarterTags.CreateTags()));
        Assert.False(await repository.SeedIfEmptyAsync(StarterTags.CreateTags()));

        var done = await repository.GetActiveAsync(CardType.Done);
        Assert.Equal(10, done.Count);
        Assert.Equal("работа", done[0].Label);
        Assert.Equal(10, await repository.GetNextOrderAsync(CardType.Done));
    }

    [Fact]
    public async Task Tags_UsedMore_ComeFirstAndArchivedAreHidden()
    {
        var repository = new TagRepository(database.Factory);
        await repository.SeedIfEmptyAsync(StarterTags.CreateTags());
        var sport = (await repository.GetActiveAsync(CardType.Done)).Single(tag => tag.Label == "спорт");
        var work = (await repository.GetActiveAsync(CardType.Done)).Single(tag => tag.Label == "работа");

        await repository.IncrementUseCountAsync([sport.Id]);
        await repository.IncrementUseCountAsync([sport.Id]);
        await repository.ArchiveAsync(work.Id);

        var active = await repository.GetActiveAsync(CardType.Done);
        Assert.Equal("спорт", active[0].Label);
        Assert.DoesNotContain(active, tag => tag.Id == work.Id);
        Assert.Single(await repository.GetByIdsAsync([work.Id]));
    }

    [Fact]
    public async Task Tags_LabelOfFourWords_Throws()
    {
        var repository = new TagRepository(database.Factory);
        var tag = TagBuilder.For(CardType.Done, "раз два три четыре").Build();

        await Assert.ThrowsAsync<ArgumentException>(() => repository.SaveAsync(tag));
    }

    [Fact]
    public async Task Drafts_SaveSameDay_Overwrites()
    {
        var repository = new DraftRepository(database.Factory);
        await repository.SaveAsync(DraftBuilder.For(Day).WithContent("{\"currentStep\":1}").Build());
        await repository.SaveAsync(DraftBuilder.For(Day).WithContent("{\"currentStep\":2}").Build());
        await repository.SaveAsync(DraftBuilder.For(Day.AddDays(1)).Build());

        Assert.Equal(2, await repository.CountAsync());
        Assert.Equal("{\"currentStep\":2}", (await repository.GetAsync(Day))!.ContentJson);
        Assert.Equal(Day.AddDays(1), (await repository.GetAllAsync())[0].DayKey);

        await repository.DeleteAsync(Day.AddDays(5));
    }

    [Fact]
    public async Task Settings_EmptyDatabase_ReturnsDefaults()
    {
        var settings = await new SettingsRepository(database.Factory).GetAsync();

        Assert.True(settings.ReminderEnabled);
        Assert.Equal(new TimeOnly(18, 0), settings.ReminderTime);
        Assert.Equal(ThemeMode.Dark, settings.Theme);
        Assert.False(settings.OnboardingDone);
    }

    [Fact]
    public async Task Settings_SaveAndGet_RoundTripsAllFields()
    {
        var repository = new SettingsRepository(database.Factory);
        var settings = new AppSettings
        {
            ReminderEnabled = false,
            ReminderTime = new TimeOnly(21, 15),
            AdaptiveReminder = false,
            Theme = ThemeMode.Auto,
            UserName = "Алексей",
            OnboardingDone = true,
            DismissedTagSuggestions = ["кофе", "зал"]
        };

        await repository.SaveAsync(settings);
        await repository.SaveAsync(settings);
        var loaded = await repository.GetAsync();

        Assert.False(loaded.ReminderEnabled);
        Assert.Equal(new TimeOnly(21, 15), loaded.ReminderTime);
        Assert.False(loaded.AdaptiveReminder);
        Assert.Equal(ThemeMode.Auto, loaded.Theme);
        Assert.Equal("Алексей", loaded.UserName);
        Assert.True(loaded.OnboardingDone);
        Assert.Equal(["кофе", "зал"], loaded.DismissedTagSuggestions);
        Assert.Equal(1, database.Scalar<int>("SELECT COUNT(*) FROM settings"));
    }

    [Fact]
    public async Task Reports_SaveWithAdvice_KeepsOrderAndRejectsDuplicateWeek()
    {
        var repository = new ReportRepository(database.Factory);
        var report = TestReportBuilder.For(Day).Build();
        var advice = new[] { "AR-30", "AR-32", "AR-34" }
            .Select(rule => ReportAdviceBuilder.For(report.Id, rule).Build())
            .ToList();

        await repository.SaveAsync(report, advice);

        Assert.Equal(["AR-30", "AR-32", "AR-34"], (await repository.GetAdviceAsync(report.Id)).Select(a => a.RuleId));
        await Assert.ThrowsAsync<SqliteException>(() => repository.SaveAsync(TestReportBuilder.For(Day).Build(), []));
        Assert.NotNull(await repository.GetByIdAsync(report.Id));
    }

    [Fact]
    public async Task Reports_FeedbackAndShown_AreStored()
    {
        var repository = new ReportRepository(database.Factory);
        var report = TestReportBuilder.For(Day).Build();
        var advice = ReportAdviceBuilder.For(report.Id, "AR-30").Build();
        await repository.SaveAsync(report, [advice]);

        Assert.Equal(report.Id, (await repository.GetUnshownAsync())!.Id);
        await repository.MarkShownAsync(report.Id, new DateTime(2026, 11, 16, 6, 0, 0, DateTimeKind.Utc));
        Assert.Null(await repository.GetUnshownAsync());

        await repository.SetAdviceFeedbackAsync(advice.Id, AdviceFeedback.NotRelevant);
        Assert.Equal(["AR-30"], await repository.GetRuleIdsWithFeedbackSinceAsync(AdviceFeedback.NotRelevant, Day));
        Assert.Empty(await repository.GetRuleIdsWithFeedbackSinceAsync(AdviceFeedback.NotRelevant, Day.AddDays(7)));

        await repository.SetAdviceFeedbackAsync(advice.Id, null);
        Assert.Empty(await repository.GetRuleIdsWithFeedbackSinceAsync(AdviceFeedback.NotRelevant, Day));
    }

    [Fact]
    public async Task Letters_SaveUpdateDelete_Works()
    {
        var repository = new LetterRepository(database.Factory);
        var later = LetterBuilder.For(Day.AddDays(10)).WithText("позже").Build();
        var sooner = LetterBuilder.For(Day.AddDays(2)).WithText("раньше").Build();
        await repository.SaveAsync(later);
        await repository.SaveAsync(sooner);

        Assert.Equal(["раньше", "позже"], (await repository.GetAllAsync()).Select(letter => letter.Text));

        sooner.OpenedAtUtc = new DateTime(2026, 11, 11, 6, 0, 0, DateTimeKind.Utc);
        sooner.Notified = true;
        await repository.SaveAsync(sooner);
        var loaded = await repository.GetByIdAsync(sooner.Id);
        Assert.Equal(sooner.OpenedAtUtc, loaded!.OpenedAtUtc);
        Assert.True(loaded.Notified);

        await repository.DeleteAsync(sooner.Id);
        Assert.Single(await repository.GetAllAsync());
    }
}
