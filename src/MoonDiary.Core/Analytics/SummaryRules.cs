namespace MoonDiary.Core.Analytics;

public static class SummaryRules
{
    public static SummaryResult Evaluate(AnalyticsInput input, AnalyticsOptions options)
    {
        var moods = input.Week.Where(day => day.Mood is not null).Select(day => day.Mood!).ToList();
        var n = moods.Count;
        var week = input.WeekStart;
        var lines = new List<string>
        {
            TemplateRenderer.Render(options.PickTemplate("AR-01", week), ("n", Text(n)))
        };

        if (n < options.MinDaysForReport)
        {
            lines.Add(options.PickTemplate("AR-05", week));

            return new SummaryResult(n, true, null, null, null, null, lines);
        }

        var energy = moods.Average(mood => mood.Energy);
        var positivity = moods.Average(mood => mood.Positivity);
        var highDays = moods.Count(mood => mood.Energy >= options.HighEnergyThreshold);

        lines.Add(TemplateRenderer.Render(
            options.PickTemplate("AR-02", week),
            ("e", TemplateRenderer.Number(energy)),
            ("p", TemplateRenderer.Number(positivity))));
        lines.Add(TemplateRenderer.Render(
            options.PickTemplate("AR-03", week),
            ("k", Text(highDays)),
            ("n", Text(n))));

        WeekComparison? comparison = null;
        var previous = input.PreviousWeek.Where(day => day.Mood is not null).Select(day => day.Mood!).ToList();

        if (previous.Count >= options.MinDaysForReport)
        {
            comparison = new WeekComparison(
                energy,
                energy - previous.Average(mood => mood.Energy),
                positivity,
                positivity - previous.Average(mood => mood.Positivity));

            lines.Add(ComparisonLine(options, week, "AR-04-energy", comparison.EnergyDelta));
            lines.Add(ComparisonLine(options, week, "AR-04-positivity", comparison.PositivityDelta));
        }

        return new SummaryResult(n, false, energy, positivity, highDays, comparison, lines);
    }

    private static string ComparisonLine(AnalyticsOptions options, DateOnly week, string ruleId, double delta) =>
        TemplateRenderer.Render(
            options.PickTemplate(ruleId, week),
            ("arrow", delta >= 0 ? "▲" : "▼"),
            ("delta", TemplateRenderer.Number(Math.Abs(delta))));

    private static string Text(int value) => value.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
