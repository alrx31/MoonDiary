using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public static class PatternRules
{
    public static PatternResult Evaluate(AnalyticsInput input, AnalyticsOptions options)
    {
        var all = new List<PatternFinding>(EvaluateTagRules(input, options));

        if (EvaluateWeekday(input, options) is { } weekday)
        {
            all.Add(weekday);
        }

        if (EvaluateCheckins(input, options) is { } checkins)
        {
            all.Add(checkins);
        }

        var lines = all
            .OrderByDescending(finding => Math.Abs(finding.Delta))
            .Take(options.PatternMaxCount)
            .Select(finding => finding.Text)
            .ToList();

        if (lines.Count == 0)
        {
            lines.Add(options.PickTemplate("AR-25", input.WeekStart));
        }

        return new PatternResult(all, lines);
    }

    public static IReadOnlyList<PatternFinding> EvaluateTagRules(AnalyticsInput input, AnalyticsOptions options)
    {
        var result = new List<PatternFinding>();
        var days = input.Period.Where(day => day.Mood is not null).ToList();
        var byDate = input.Period.ToDictionary(day => day.Date);
        var tagIds = input.Period
            .SelectMany(day => day.Tags.Select(tag => tag.TagId))
            .Distinct()
            .Where(id => input.Tags.TryGetValue(id, out var tag) && tag.Direction != LifeDirection.None)
            .OrderBy(id => input.Tags[id].CardType)
            .ThenBy(id => input.Tags[id].Order)
            .ToList();

        foreach (var tagId in tagIds)
        {
            var tag = input.Tags[tagId];
            var with = days.Where(day => day.HasTag(tagId)).ToList();
            var without = days.Where(day => !day.HasTag(tagId)).ToList();

            AddIfSignificant(
                result, input, options, "AR-20", tag, MoodScale.Positivity,
                with.Select(day => (double)day.Mood!.Positivity).ToList(),
                without.Select(day => (double)day.Mood!.Positivity).ToList());
            AddIfSignificant(
                result, input, options, "AR-21", tag, MoodScale.Energy,
                with.Select(day => (double)day.Mood!.Energy).ToList(),
                without.Select(day => (double)day.Mood!.Energy).ToList());

            var after = days
                .Where(day => byDate.TryGetValue(day.Date.AddDays(-1), out var previous) && previous.HasTag(tagId))
                .ToList();
            var rest = days.Except(after).ToList();

            AddIfSignificant(
                result, input, options, "AR-22", tag, MoodScale.Positivity,
                after.Select(day => (double)day.Mood!.Positivity).ToList(),
                rest.Select(day => (double)day.Mood!.Positivity).ToList());
        }

        return result;
    }

    private static void AddIfSignificant(
        List<PatternFinding> result,
        AnalyticsInput input,
        AnalyticsOptions options,
        string ruleId,
        Tag tag,
        MoodScale scale,
        List<double> group,
        List<double> other)
    {
        if (group.Count < options.PatternMinGroupDays || other.Count < options.PatternMinGroupDays)
        {
            return;
        }

        var delta = group.Average() - other.Average();

        if (Math.Abs(delta) < options.PatternMinDelta)
        {
            return;
        }

        var text = TemplateRenderer.Render(
            options.PickTemplate(ruleId, input.WeekStart),
            ("tag", tag.Label),
            ("direction", delta > 0 ? "выше" : "ниже"),
            ("delta", TemplateRenderer.Number(Math.Abs(delta))));

        result.Add(new PatternFinding(ruleId, tag.Id, scale, delta, text));
    }

    private static PatternFinding? EvaluateWeekday(AnalyticsInput input, AnalyticsOptions options)
    {
        var groups = input.Period
            .Where(day => day.Mood is not null)
            .GroupBy(day => AnalyticsOptions.WeekdayIndex(day.Date))
            .ToDictionary(group => group.Key, group => group.Select(day => (double)day.Mood!.Energy).ToList());

        if (groups.Count < 7 || groups.Values.Any(values => values.Count < options.WeekdayMinValues))
        {
            return null;
        }

        var lowest = groups.OrderBy(group => group.Value.Average()).ThenBy(group => group.Key).First();
        var strength = groups.Values.SelectMany(values => values).Average() - lowest.Value.Average();

        if (strength < options.PatternMinDelta)
        {
            return null;
        }

        var text = TemplateRenderer.Render(
            options.PickTemplate("AR-23", input.WeekStart),
            ("weekday", options.WeekdayPhrases[lowest.Key]));

        return new PatternFinding("AR-23", null, MoodScale.Energy, strength, text);
    }

    private static PatternFinding? EvaluateCheckins(AnalyticsInput input, AnalyticsOptions options)
    {
        var differences = input.Period
            .Where(day => day.Evening is not null && day.Checkins.Count > 0)
            .Select(day => day.Evening!.Positivity - day.Checkins.Average(checkin => checkin.Positivity))
            .ToList();

        if (differences.Count < options.CheckinMinDays)
        {
            return null;
        }

        var delta = differences.Average();

        if (delta < options.PatternMinDelta)
        {
            return null;
        }

        var text = TemplateRenderer.Render(
            options.PickTemplate("AR-24", input.WeekStart),
            ("delta", TemplateRenderer.Number(delta)));

        return new PatternFinding("AR-24", null, MoodScale.Positivity, delta, text);
    }
}
