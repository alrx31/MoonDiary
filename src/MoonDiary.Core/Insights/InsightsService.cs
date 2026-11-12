using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Insights;

public sealed class InsightsService
{
    public const int DynamicsDays = 28;

    private readonly IEntryRepository entries;
    private readonly DayClock clock;

    public InsightsService(IEntryRepository entries, DayClock clock)
    {
        this.entries = entries;
        this.clock = clock;
    }

    public async Task<int> GetStreakAsync() =>
        StreakCalculator.Calculate((await entries.GetEveningDaysAsync()).ToHashSet(), clock.CurrentDayKey);

    public async Task<IReadOnlyList<DynamicsPoint>> GetDynamicsAsync()
    {
        var today = clock.CurrentDayKey;
        var first = today.AddDays(-(DynamicsDays - 1));
        var all = await entries.GetRangeAsync(first, today);
        var evenings = all.Where(entry => entry.Type == EntryType.Evening).ToDictionary(entry => entry.DayKey);
        var checkins = all.Where(entry => entry.Type == EntryType.Checkin).ToLookup(entry => entry.DayKey);
        var result = new List<DynamicsPoint>();

        for (var date = first; date <= today; date = date.AddDays(1))
        {
            evenings.TryGetValue(date, out var evening);
            result.Add(new DynamicsPoint(date, DayValueCalculator.Calculate(evening, checkins[date].ToList())));
        }

        return result;
    }

    public async Task<IReadOnlyList<MonthAchievement>> GetMonthAchievementsAsync(int year, int month)
    {
        var first = new DateOnly(year, month, 1);
        var all = await entries.GetRangeAsync(first, first.AddMonths(1).AddDays(-1));

        return all
            .Where(entry => entry.Type == EntryType.Evening && !string.IsNullOrWhiteSpace(entry.Achievement))
            .OrderByDescending(entry => entry.DayKey)
            .Select(entry => new MonthAchievement(entry.DayKey, entry.Achievement!.Trim()))
            .ToList();
    }
}
