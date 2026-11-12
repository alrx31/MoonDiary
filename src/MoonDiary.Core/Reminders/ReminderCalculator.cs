using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Reminders;

public static class ReminderCalculator
{
    public const int AdaptiveMinEntries = 14;

    public static readonly TimeSpan AdaptiveLead = TimeSpan.FromMinutes(10);

    public static TimeSpan GetFillOffset(Entry entry)
    {
        var local = DayClock.ToLocal(entry.CreatedAtUtc, entry.TimezoneOffsetMinutes);
        var time = local.TimeOfDay;

        return time < DayClock.DayStart.ToTimeSpan() ? time + TimeSpan.FromHours(24) : time;
    }

    public static TimeSpan GetReminderOffset(AppSettings settings, IReadOnlyList<Entry> latestEvenings)
    {
        if (!settings.AdaptiveReminder || latestEvenings.Count < AdaptiveMinEntries)
        {
            return settings.ReminderTime.ToTimeSpan();
        }

        var offsets = latestEvenings
            .Take(AdaptiveMinEntries)
            .Select(GetFillOffset)
            .OrderBy(offset => offset)
            .ToList();
        var middle = offsets.Count / 2;
        var median = offsets.Count % 2 == 0
            ? TimeSpan.FromTicks((offsets[middle - 1].Ticks + offsets[middle].Ticks) / 2)
            : offsets[middle];
        var result = median - AdaptiveLead;

        return TimeSpan.FromMinutes(Math.Floor(result.TotalMinutes));
    }

    public static DateTimeOffset? GetNext(
        DateTimeOffset localNow,
        DateOnly currentDayKey,
        bool hasEveningForCurrentDay,
        AppSettings settings,
        IReadOnlyList<Entry> latestEvenings)
    {
        if (!settings.ReminderEnabled)
        {
            return null;
        }

        var offset = GetReminderOffset(settings, latestEvenings);
        var today = Moment(currentDayKey, offset, localNow.Offset);

        return !hasEveningForCurrentDay && today > localNow
            ? today
            : Moment(currentDayKey.AddDays(1), offset, localNow.Offset);
    }

    private static DateTimeOffset Moment(DateOnly day, TimeSpan offset, TimeSpan zone) =>
        new DateTimeOffset(day.ToDateTime(TimeOnly.MinValue), zone) + offset;
}
