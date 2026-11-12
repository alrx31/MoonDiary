using MoonDiary.Core.Analytics;
using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Reports;

public static class ReportFactory
{
    public static (Report Report, IReadOnlyList<ReportAdvice> Advice) Build(
        AnalyticsInput input,
        AnalyticsOptions options,
        IReadOnlyCollection<string> excludedRuleIds,
        DateTime nowUtc)
    {
        var summary = SummaryRules.Evaluate(input, options);
        var report = new Report
        {
            Id = Guid.NewGuid(),
            WeekStart = input.WeekStart,
            GeneratedAtUtc = nowUtc,
            DaysWithEntries = summary.DaysWithEntries,
            Status = summary.Insufficient ? ReportStatus.InsufficientData : ReportStatus.Ok
        };

        if (summary.Insufficient)
        {
            report.ContentJson = ReportContent.Serialize(new ReportContent(summary.Lines, [], [], null, true));

            return (report, []);
        }

        var achievements = AchievementRules.Evaluate(input, options, summary.DaysWithEntries);
        var patterns = PatternRules.Evaluate(input, options);
        var candidates = AdviceRules.Evaluate(input, summary, patterns, options);
        var advice = AdviceSelector.Select(candidates, excludedRuleIds, options.AdviceMaxCount)
            .Select(candidate => new ReportAdvice
            {
                Id = Guid.NewGuid(),
                ReportId = report.Id,
                RuleId = candidate.RuleId,
                Direction = candidate.Direction,
                Text = candidate.Text
            })
            .ToList();

        report.ContentJson = ReportContent.Serialize(new ReportContent(
            summary.Lines,
            achievements.Lines,
            patterns.Lines,
            summary.Comparison,
            false));

        return (report, advice);
    }
}
