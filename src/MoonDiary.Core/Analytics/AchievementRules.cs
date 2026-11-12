using System.Globalization;
using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public static class AchievementRules
{
    public static AchievementResult Evaluate(AnalyticsInput input, AnalyticsOptions options, int daysWithEntries)
    {
        var week = input.WeekStart;
        var lines = new List<string>();

        foreach (var day in input.Week)
        {
            var text = day.Evening?.Achievement?.Trim();

            if (!string.IsNullOrEmpty(text))
            {
                lines.Add(TemplateRenderer.Render(
                    options.PickTemplate("AR-10", week),
                    ("day", options.WeekdayName(day.Date)),
                    ("text", text)));
            }
        }

        var achievementLinks = input.Week
            .SelectMany(day => day.Tags.Where(tag => tag.CardType == CardType.Achievement))
            .Where(link => input.Tags.ContainsKey(link.TagId))
            .ToList();
        var top = achievementLinks
            .GroupBy(link => link.TagId)
            .Select(group => (Tag: input.Tags[group.Key], Count: group.Count()))
            .Where(item => item.Count >= options.AchievementTagMinCount)
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Tag.Order)
            .FirstOrDefault();

        if (top.Tag is not null)
        {
            lines.Add(TemplateRenderer.Render(
                options.PickTemplate("AR-11", week),
                ("k", top.Count.ToString(CultureInfo.InvariantCulture)),
                ("tag", top.Tag.Label)));
        }

        if (lines.Count == 0 && achievementLinks.Count == 0)
        {
            lines.Add(TemplateRenderer.Render(
                options.PickTemplate("AR-12", week),
                ("n", daysWithEntries.ToString(CultureInfo.InvariantCulture))));
        }

        return new AchievementResult(lines, top.Tag?.Id);
    }
}
