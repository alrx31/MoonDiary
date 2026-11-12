namespace MoonDiary.Core.Analytics;

public sealed record PatternResult(IReadOnlyList<PatternFinding> All, IReadOnlyList<string> Lines);
