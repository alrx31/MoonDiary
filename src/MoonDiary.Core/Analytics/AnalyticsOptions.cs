using MoonDiary.Core.Resources;

namespace MoonDiary.Core.Analytics;

public sealed class AnalyticsOptions
{
    private static readonly Lazy<AnalyticsOptions> Loaded =
        new(() => ResourceStore.ReadJson<AnalyticsOptions>("analytics-rules.json"));

    public static AnalyticsOptions Default => Loaded.Value;

    public int MinDaysForReport { get; set; }

    public int HighEnergyThreshold { get; set; }

    public int PatternMinGroupDays { get; set; }

    public double PatternMinDelta { get; set; }

    public int PatternMaxCount { get; set; }

    public int WeekdayMinValues { get; set; }

    public int CheckinMinDays { get; set; }

    public int AchievementTagMinCount { get; set; }

    public int AdviceMaxCount { get; set; }

    public int AdviceNotRelevantWeeks { get; set; }

    public int SleepSignalMinDays { get; set; }

    public int SleepLowEnergyMax { get; set; }

    public int WorkSignalMinDays { get; set; }

    public double WorkMaxAvgPositivity { get; set; }

    public int LonelyMinDays { get; set; }

    public List<string> LonelyLabels { get; set; } = [];

    public int AnxietyMinDays { get; set; }

    public List<string> AnxietyLabels { get; set; } = [];

    public List<string> WeekdayNames { get; set; } = [];

    public List<string> WeekdayPhrases { get; set; } = [];

    public Dictionary<string, List<string>> Templates { get; set; } = [];

    public string PickTemplate(string ruleId, DateOnly weekStart)
    {
        if (!Templates.TryGetValue(ruleId, out var variants) || variants.Count == 0)
        {
            throw new KeyNotFoundException($"Нет шаблона для правила {ruleId}.");
        }

        return variants[weekStart.DayNumber / 7 % variants.Count];
    }

    public string WeekdayName(DateOnly date) => WeekdayNames[WeekdayIndex(date)];

    public static int WeekdayIndex(DateOnly date) => ((int)date.DayOfWeek + 6) % 7;
}
