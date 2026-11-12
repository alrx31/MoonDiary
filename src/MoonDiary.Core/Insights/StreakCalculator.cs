namespace MoonDiary.Core.Insights;

public static class StreakCalculator
{
    public static int Calculate(IReadOnlySet<DateOnly> eveningDays, DateOnly today)
    {
        var day = eveningDays.Contains(today) ? today : today.AddDays(-1);
        var streak = 0;

        while (eveningDays.Contains(day))
        {
            streak++;
            day = day.AddDays(-1);
        }

        return streak;
    }
}
