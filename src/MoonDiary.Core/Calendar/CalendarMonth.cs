namespace MoonDiary.Core.Calendar;

public sealed record CalendarMonth(
    int Year,
    int Month,
    IReadOnlyList<CalendarDay> Days,
    bool CanGoNext,
    bool CanGoPrevious);
