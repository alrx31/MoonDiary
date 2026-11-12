namespace MoonDiary.Core.Analytics;

public static class AdviceSelector
{
    public static IReadOnlyList<AdviceCandidate> Select(
        IReadOnlyList<AdviceCandidate> candidates,
        IReadOnlyCollection<string> excludedRuleIds,
        int maxCount)
    {
        var ordered = candidates
            .Where(candidate => !excludedRuleIds.Contains(candidate.RuleId))
            .OrderByDescending(candidate => candidate.Strength)
            .ToList();
        var result = ordered
            .GroupBy(candidate => candidate.Direction)
            .Select(group => group.First())
            .OrderByDescending(candidate => candidate.Strength)
            .Take(maxCount)
            .ToList();

        foreach (var candidate in ordered)
        {
            if (result.Count >= maxCount)
            {
                break;
            }

            if (!result.Contains(candidate))
            {
                result.Add(candidate);
            }
        }

        return result;
    }
}
