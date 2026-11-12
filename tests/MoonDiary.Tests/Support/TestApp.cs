using Microsoft.Extensions.Time.Testing;
using MoonDiary.Core.Analytics;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Insights;
using MoonDiary.Core.Letters;
using MoonDiary.Core.Reminders;
using MoonDiary.Core.Reports;
using MoonDiary.Core.Startup;
using MoonDiary.Core.Tags;
using MoonDiary.Core.Time;
using MoonDiary.Core.Transfer;
using MoonDiary.Data;
using MoonDiary.Data.Repositories;

namespace MoonDiary.Tests.Support;

public sealed class TestApp : IDisposable
{
    public TestApp(FakeTimeProvider time)
    {
        Time = time;
        Database = new TestDatabase();
        Clock = new DayClock(time);
        Entries = new EntryRepository(Database.Factory);
        Tags = new TagRepository(Database.Factory);
        Drafts = new DraftRepository(Database.Factory);
        Settings = new SettingsRepository(Database.Factory);
        Reports = new ReportRepository(Database.Factory);
        Letters = new LetterRepository(Database.Factory);
        Transfer = new DataTransferRepository(Database.Factory);
        TagService = new TagService(Tags);
        EntryService = new EntryService(Entries, Tags, Drafts, Clock);
        CheckinService = new CheckinService(Entries, Clock);
        CalendarService = new CalendarService(Entries, Tags, Clock);
        StartupService = new StartupService(Settings, Entries, Reports, Clock);
        InputBuilder = new AnalyticsInputBuilder(Entries, Tags);
        ReportService = new WeeklyReportService(Reports, InputBuilder, AnalyticsOptions.Default, Clock);
        TransferService = new DataTransferService(Transfer, Clock, MigrationRunner.LatestVersion);
        ReminderService = new ReminderService(Settings, Entries, Clock);
        LetterService = new LetterService(Letters, Clock);
        InsightsService = new InsightsService(Entries, Clock);
        SuggestionService = new TagSuggestionService(Entries, Tags, Settings, Clock);
    }

    public FakeTimeProvider Time { get; }

    public TestDatabase Database { get; }

    public DayClock Clock { get; }

    public EntryRepository Entries { get; }

    public TagRepository Tags { get; }

    public DraftRepository Drafts { get; }

    public SettingsRepository Settings { get; }

    public ReportRepository Reports { get; }

    public LetterRepository Letters { get; }

    public DataTransferRepository Transfer { get; }

    public TagService TagService { get; }

    public EntryService EntryService { get; }

    public CheckinService CheckinService { get; }

    public CalendarService CalendarService { get; }

    public StartupService StartupService { get; }

    public AnalyticsInputBuilder InputBuilder { get; }

    public WeeklyReportService ReportService { get; }

    public DataTransferService TransferService { get; }

    public ReminderService ReminderService { get; }

    public LetterService LetterService { get; }

    public InsightsService InsightsService { get; }

    public TagSuggestionService SuggestionService { get; }

    public static TestApp At(int year, int month, int day, int hour, int minute) =>
        new(TestTime.At(year, month, day, hour, minute));

    public async Task<TestApp> SeedTagsAsync()
    {
        await TagService.SeedStarterTagsAsync();

        return this;
    }

    public async Task<Tag> TagAsync(CardType cardType, string label) =>
        (await Tags.GetActiveAsync(cardType)).Single(tag => tag.Label == label);

    public async Task<Entry> EveningAsync(DateOnly day, int energy, int positivity, params (CardType Card, string Label)[] tags)
    {
        var state = await EntryService.OpenWizardAsync(day);
        state.SetEnergy(energy);
        state.SetPositivity(positivity);

        foreach (var (card, label) in tags)
        {
            var tag = await TagAsync(card, label);

            if (!state.IsTagSelected(card, tag))
            {
                state.ToggleTag(card, tag);
            }
        }

        return await EntryService.SaveEveningAsync(state);
    }

    public void SetLocal(int year, int month, int day, int hour, int minute) =>
        Time.SetUtcNow(new DateTimeOffset(year, month, day, hour, minute, 0, TestTime.Minsk).ToUniversalTime());

    public void Dispose() => Database.Dispose();
}
