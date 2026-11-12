using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Calendar;

public static class DayValueCalculator
{
    public static DayMood? Calculate(Entry? evening, IReadOnlyCollection<Entry> checkins)
    {
        if (evening is not null)
        {
            return new DayMood(evening.Energy, evening.Positivity, true);
        }

        if (checkins.Count == 0)
        {
            return null;
        }

        return new DayMood(
            Round(checkins.Average(entry => entry.Energy)),
            Round(checkins.Average(entry => entry.Positivity)),
            false);
    }

    private static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);
}
