using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Calendar;

public sealed record DayDetails(
    DateOnly Date,
    Entry? Evening,
    IReadOnlyList<Entry> Checkins,
    DayMood? Mood,
    IReadOnlyList<Tag> EveningTags,
    bool IsToday,
    bool IsFuture);
