namespace MoonDiary.Core.Calendar;

public sealed record CalendarDay(
    DateOnly Date,
    DayMood? Mood,
    bool HasEvening,
    int CheckinCount,
    bool IsToday,
    bool IsFuture,
    string? ShortText);
