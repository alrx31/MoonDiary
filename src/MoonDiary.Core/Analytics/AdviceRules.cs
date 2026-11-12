using System.Globalization;
using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public static class AdviceRules
{
    public static IReadOnlyList<AdviceCandidate> Evaluate(
        AnalyticsInput input,
        SummaryResult summary,
        PatternResult patterns,
        AnalyticsOptions options)
    {
        var result = new List<AdviceCandidate>();

        if (summary.Insufficient)
        {
            return result;
        }

        var week = input.WeekStart;
        var sleepTagDays = DaysWithDirection(input, LifeDirection.Sleep);
        var lowEnergyDays = input.Week.Count(day => day.Mood is not null && day.Mood.Energy <= options.SleepLowEnergyMax);

        if (sleepTagDays >= options.SleepSignalMinDays || lowEnergyDays >= options.SleepSignalMinDays)
        {
            result.Add(new AdviceCandidate(
                "AR-30",
                LifeDirection.Sleep,
                Math.Max(sleepTagDays, lowEnergyDays) / 7.0,
                options.PickTemplate("AR-30", week)));
        }

        var healthPattern = BestPattern(input, patterns, "AR-20", LifeDirection.Health);

        if (healthPattern is not null)
        {
            var tag = input.Tags[healthPattern.TagId!.Value];
            var k = Math.Clamp(input.Week.Count(day => day.HasTag(tag.Id)) + 1, 2, 7);

            result.Add(new AdviceCandidate(
                "AR-31",
                LifeDirection.Health,
                healthPattern.Delta / 2,
                TemplateRenderer.Render(
                    options.PickTemplate("AR-31", week),
                    ("tag", tag.Label),
                    ("k", k.ToString(CultureInfo.InvariantCulture)))));
        }

        if (DaysWithDirection(input, LifeDirection.Health) == 0)
        {
            result.Add(new AdviceCandidate("AR-32", LifeDirection.Health, 0.5, options.PickTemplate("AR-32", week)));
        }

        var workDays = DaysWithDirection(input, LifeDirection.Work);

        if (workDays >= options.WorkSignalMinDays && summary.AveragePositivity <= options.WorkMaxAvgPositivity)
        {
            result.Add(new AdviceCandidate(
                "AR-33",
                LifeDirection.Work,
                workDays / 7.0,
                options.PickTemplate("AR-33", week)));
        }

        var lonelyDays = DaysWithLabels(input, options.LonelyLabels);

        if (lonelyDays >= options.LonelyMinDays)
        {
            result.Add(new AdviceCandidate(
                "AR-34",
                LifeDirection.Relations,
                lonelyDays / 7.0,
                options.PickTemplate("AR-34", week)));
        }

        var anxietyDays = DaysWithLabels(input, options.AnxietyLabels);

        if (anxietyDays >= options.AnxietyMinDays)
        {
            result.Add(new AdviceCandidate(
                "AR-35",
                LifeDirection.Emotions,
                anxietyDays / 7.0,
                options.PickTemplate("AR-35", week)));
        }

        var householdPattern = BestPattern(input, patterns, "AR-22", LifeDirection.Household);

        if (householdPattern is not null)
        {
            result.Add(new AdviceCandidate(
                "AR-36",
                LifeDirection.Household,
                householdPattern.Delta / 2,
                TemplateRenderer.Render(
                    options.PickTemplate("AR-36", week),
                    ("tag", input.Tags[householdPattern.TagId!.Value].Label))));
        }

        return result;
    }

    private static PatternFinding? BestPattern(
        AnalyticsInput input,
        PatternResult patterns,
        string ruleId,
        LifeDirection direction) =>
        patterns.All
            .Where(finding => finding.RuleId == ruleId && finding.Delta > 0 && finding.TagId is not null)
            .Where(finding => input.Tags.TryGetValue(finding.TagId!.Value, out var tag) && tag.Direction == direction)
            .OrderByDescending(finding => finding.Delta)
            .FirstOrDefault();

    private static int DaysWithDirection(AnalyticsInput input, LifeDirection direction) =>
        input.Week.Count(day => day.Tags.Any(link =>
            input.Tags.TryGetValue(link.TagId, out var tag) && tag.Direction == direction));

    private static int DaysWithLabels(AnalyticsInput input, List<string> labels) =>
        input.Week.Count(day => day.Tags.Any(link =>
            input.Tags.TryGetValue(link.TagId, out var tag)
            && labels.Contains(tag.Label, StringComparer.OrdinalIgnoreCase)));
}
