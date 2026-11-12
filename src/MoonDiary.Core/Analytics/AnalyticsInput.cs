using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public sealed record AnalyticsInput(
    DateOnly WeekStart,
    IReadOnlyList<AnalyticsDay> Week,
    IReadOnlyList<AnalyticsDay> PreviousWeek,
    IReadOnlyList<AnalyticsDay> Period,
    IReadOnlyDictionary<Guid, Tag> Tags);
