namespace MoonDiary.Core.Analytics;

public sealed record AchievementResult(IReadOnlyList<string> Lines, Guid? TopTagId);
