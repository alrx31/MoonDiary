using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Reminders;

public sealed class ReminderService
{
    private readonly ISettingsRepository settings;
    private readonly IEntryRepository entries;
    private readonly DayClock clock;

    public ReminderService(ISettingsRepository settings, IEntryRepository entries, DayClock clock)
    {
        this.settings = settings;
        this.entries = entries;
        this.clock = clock;
    }

    public async Task<DateTimeOffset?> GetNextReminderAsync() =>
        ReminderCalculator.GetNext(
            clock.LocalNow,
            clock.CurrentDayKey,
            await entries.HasEveningAsync(clock.CurrentDayKey),
            await settings.GetAsync(),
            await entries.GetLatestEveningsAsync(ReminderCalculator.AdaptiveMinEntries, excludeBackfilled: true));

    public async Task<TimeSpan> GetCurrentOffsetAsync() =>
        ReminderCalculator.GetReminderOffset(
            await settings.GetAsync(),
            await entries.GetLatestEveningsAsync(ReminderCalculator.AdaptiveMinEntries, excludeBackfilled: true));
}
