using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Reports;
using MoonDiary.Core.Tags;
using MoonDiary.Core.Time;

namespace MoonDiary.Services;

public sealed class DebugSeeder
{
    private readonly ISettingsRepository settings;
    private readonly IEntryRepository entries;
    private readonly EntryService entryService;
    private readonly TagService tags;
    private readonly WeeklyReportService reports;
    private readonly DayClock clock;

    public DebugSeeder(
        ISettingsRepository settings,
        IEntryRepository entries,
        EntryService entryService,
        TagService tags,
        WeeklyReportService reports,
        DayClock clock)
    {
        this.settings = settings;
        this.entries = entries;
        this.entryService = entryService;
        this.tags = tags;
        this.reports = reports;
        this.clock = clock;
    }

    public async Task SeedAsync(string scenario)
    {
        var current = await settings.GetAsync();

        if (current.OnboardingDone)
        {
            return;
        }

        current.OnboardingDone = true;
        await settings.SaveAsync(current);

        var names = scenario.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (names.Contains("month") || names.Contains("report"))
        {
            await SeedMonthAsync();
        }

        if (names.Contains("report"))
        {
            await reports.EnsureLastWeekReportAsync();
        }

        if (names.Contains("shown") && await reports.EnsureLastWeekReportAsync() is { } report)
        {
            await reports.MarkShownAsync(report.Id);
        }
    }

    private async Task SeedMonthAsync()
    {
        var today = clock.CurrentDayKey;
        var done = await tags.GetSuggestionsForCardAsync(CardType.Done);
        var achievements = await tags.GetSuggestionsForCardAsync(CardType.Achievement);
        var feelings = await tags.GetSuggestionsForCardAsync(CardType.Feelings);

        for (var offset = 1; offset <= 34; offset++)
        {
            if (offset % 9 == 0)
            {
                continue;
            }

            var day = today.AddDays(-offset);
            var state = await entryService.OpenWizardAsync(day);
            var sport = offset % 3 == 0;

            state.SetEnergy(sport ? 4 + (offset % 2) : 2 + (offset % 3));
            state.SetPositivity(sport ? 5 : 2 + (offset % 2));
            state.ToggleTag(CardType.Done, done[0]);

            if (sport)
            {
                state.ToggleTag(CardType.Done, done[2]);
            }

            if (offset % 4 == 0)
            {
                state.ToggleTag(CardType.Achievement, achievements[offset % achievements.Count]);
            }

            state.ToggleTag(CardType.Feelings, feelings[offset % feelings.Count]);

            var entry = await entryService.SaveEveningAsync(state);
            entry.IsBackfilled = false;
            entry.CreatedAtUtc = clock.GetDayStartLocal(day).AddHours(17).UtcDateTime;
            entry.UpdatedAtUtc = entry.CreatedAtUtc;
            await entries.SaveAsync(entry);
        }
    }
}
