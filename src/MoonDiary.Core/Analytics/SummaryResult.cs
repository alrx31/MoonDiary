namespace MoonDiary.Core.Analytics;

public sealed record SummaryResult(
    int DaysWithEntries,
    bool Insufficient,
    double? AverageEnergy,
    double? AveragePositivity,
    int? HighEnergyDays,
    WeekComparison? Comparison,
    IReadOnlyList<string> Lines);
