using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Analytics;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Reports;

public sealed class WeeklyReportService
{
    private readonly IReportRepository reports;
    private readonly AnalyticsInputBuilder inputBuilder;
    private readonly AnalyticsOptions options;
    private readonly DayClock clock;

    public WeeklyReportService(
        IReportRepository reports,
        AnalyticsInputBuilder inputBuilder,
        AnalyticsOptions options,
        DayClock clock)
    {
        this.reports = reports;
        this.inputBuilder = inputBuilder;
        this.options = options;
        this.clock = clock;
    }

    public async Task<ReportView?> EnsureLastWeekReportAsync()
    {
        var weekStart = clock.LastCompletedWeekStart;

        if (await reports.GetByWeekStartAsync(weekStart) is not null)
        {
            return null;
        }

        var input = await inputBuilder.BuildAsync(weekStart);
        var excluded = await reports.GetRuleIdsWithFeedbackSinceAsync(
            AdviceFeedback.NotRelevant,
            weekStart.AddDays(-7 * options.AdviceNotRelevantWeeks));
        var (report, advice) = ReportFactory.Build(input, options, excluded, clock.UtcNow);

        await reports.SaveAsync(report, advice);

        return ToView(report, advice);
    }

    public async Task<ReportView?> GetAsync(Guid id)
    {
        var report = await reports.GetByIdAsync(id);

        return report is null ? null : ToView(report, await reports.GetAdviceAsync(id));
    }

    public async Task<ReportView?> GetLatestAsync()
    {
        var report = await reports.GetLatestAsync();

        return report is null ? null : ToView(report, await reports.GetAdviceAsync(report.Id));
    }

    public async Task<IReadOnlyList<ReportView>> GetHistoryAsync() =>
        (await reports.GetAllAsync()).Select(report => ToView(report, [])).ToList();

    public async Task<Guid?> GetUnshownIdAsync() => (await reports.GetUnshownAsync())?.Id;

    public Task MarkShownAsync(Guid id) => reports.MarkShownAsync(id, clock.UtcNow);

    public Task SetFeedbackAsync(Guid adviceId, AdviceFeedback? feedback) =>
        reports.SetAdviceFeedbackAsync(adviceId, feedback);

    private static ReportView ToView(Report report, IReadOnlyList<ReportAdvice> advice) => new(
        report.Id,
        report.WeekStart,
        report.WeekStart.AddDays(6),
        report.Status,
        report.DaysWithEntries,
        ReportContent.Deserialize(report.ContentJson),
        advice,
        report.ShownAtUtc is not null);
}
