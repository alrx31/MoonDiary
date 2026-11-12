using MoonDiary.Core.Calendar;
using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public sealed record AnalyticsDay(
    DateOnly Date,
    DayMood? Mood,
    Entry? Evening,
    IReadOnlyList<Entry> Checkins,
    IReadOnlyList<EntryTag> Tags)
{
    public bool HasTag(Guid tagId) => Tags.Any(tag => tag.TagId == tagId);
}
