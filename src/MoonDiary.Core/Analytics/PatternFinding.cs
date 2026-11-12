using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public sealed record PatternFinding(string RuleId, Guid? TagId, MoodScale Scale, double Delta, string Text);
