using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public sealed record AdviceCandidate(string RuleId, LifeDirection Direction, double Strength, string Text);
