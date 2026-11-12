using Microsoft.Data.Sqlite;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Data.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private const string Columns = "id, week_start, generated_at_utc, status, days_with_entries, content, shown_at_utc";

    private readonly SqliteConnectionFactory factory;

    public ReportRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<Report?> GetByIdAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM reports WHERE id = $id", ("$id", SqlValues.FromGuid(id)))
            .FirstOrDefault();
    });

    public Task<Report?> GetByWeekStartAsync(DateOnly weekStart) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(
                Map,
                $"SELECT {Columns} FROM reports WHERE week_start = $week",
                ("$week", SqlValues.FromDateOnly(weekStart)))
            .FirstOrDefault();
    });

    public Task<IReadOnlyList<Report>> GetAllAsync() => Task.Run<IReadOnlyList<Report>>(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM reports ORDER BY week_start DESC");
    });

    public Task<Report?> GetLatestAsync() => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM reports ORDER BY week_start DESC LIMIT 1").FirstOrDefault();
    });

    public Task<Report?> GetUnshownAsync() => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(
                Map,
                $"SELECT {Columns} FROM reports WHERE shown_at_utc IS NULL ORDER BY week_start DESC LIMIT 1")
            .FirstOrDefault();
    });

    public Task SaveAsync(Report report, IReadOnlyList<ReportAdvice> advice) => Task.Run(() =>
    {
        using var c = factory.Open();
        using var transaction = c.BeginTransaction();

        Insert(c, transaction, report);

        for (var index = 0; index < advice.Count; index++)
        {
            InsertAdvice(c, transaction, advice[index], index);
        }

        transaction.Commit();
    });

    public Task MarkShownAsync(Guid id, DateTime shownAtUtc) => Task.Run(() =>
    {
        using var c = factory.Open();
        c.Execute(
            "UPDATE reports SET shown_at_utc = $shown WHERE id = $id",
            ("$shown", SqlValues.FromUtcDateTime(shownAtUtc)),
            ("$id", SqlValues.FromGuid(id)));
    });

    public Task<IReadOnlyList<ReportAdvice>> GetAdviceAsync(Guid reportId) =>
        Task.Run<IReadOnlyList<ReportAdvice>>(() =>
        {
            using var c = factory.Open();

            return c.Query(
                MapAdvice,
                "SELECT id, report_id, rule_id, direction, text, feedback FROM report_advice " +
                "WHERE report_id = $report ORDER BY sort_order",
                ("$report", SqlValues.FromGuid(reportId)));
        });

    public Task SetAdviceFeedbackAsync(Guid adviceId, AdviceFeedback? feedback) => Task.Run(() =>
    {
        using var c = factory.Open();
        c.Execute(
            "UPDATE report_advice SET feedback = $feedback WHERE id = $id",
            ("$feedback", feedback is null ? null : SqlValues.FromEnum(feedback.Value)),
            ("$id", SqlValues.FromGuid(adviceId)));
    });

    public Task<IReadOnlyList<string>> GetRuleIdsWithFeedbackSinceAsync(AdviceFeedback feedback, DateOnly minWeekStart) =>
        Task.Run<IReadOnlyList<string>>(() =>
        {
            using var c = factory.Open();

            return c.Query(
                reader => reader.GetText("rule_id"),
                "SELECT DISTINCT a.rule_id FROM report_advice a JOIN reports r ON r.id = a.report_id " +
                "WHERE a.feedback = $feedback AND r.week_start >= $week",
                ("$feedback", SqlValues.FromEnum(feedback)),
                ("$week", SqlValues.FromDateOnly(minWeekStart)));
        });

    internal static void Insert(SqliteConnection c, SqliteTransaction? transaction, Report report) =>
        c.Execute(
            transaction,
            $"INSERT INTO reports ({Columns}) VALUES ($id, $week, $generated, $status, $days, $content, $shown)",
            ("$id", SqlValues.FromGuid(report.Id)),
            ("$week", SqlValues.FromDateOnly(report.WeekStart)),
            ("$generated", SqlValues.FromUtcDateTime(report.GeneratedAtUtc)),
            ("$status", SqlValues.FromEnum(report.Status)),
            ("$days", report.DaysWithEntries),
            ("$content", report.ContentJson),
            ("$shown", report.ShownAtUtc is null ? null : SqlValues.FromUtcDateTime(report.ShownAtUtc.Value)));

    internal static void InsertAdvice(SqliteConnection c, SqliteTransaction? transaction, ReportAdvice advice, int order) =>
        c.Execute(
            transaction,
            "INSERT INTO report_advice (id, report_id, rule_id, direction, text, feedback, sort_order) " +
            "VALUES ($id, $report, $rule, $direction, $text, $feedback, $order)",
            ("$id", SqlValues.FromGuid(advice.Id)),
            ("$report", SqlValues.FromGuid(advice.ReportId)),
            ("$rule", advice.RuleId),
            ("$direction", SqlValues.FromEnum(advice.Direction)),
            ("$text", advice.Text),
            ("$feedback", advice.Feedback is null ? null : SqlValues.FromEnum(advice.Feedback.Value)),
            ("$order", order));

    internal static Report Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetGuid("id"),
        WeekStart = reader.GetDateOnly("week_start"),
        GeneratedAtUtc = reader.GetUtcDateTime("generated_at_utc"),
        Status = reader.GetEnum<ReportStatus>("status"),
        DaysWithEntries = reader.GetInt("days_with_entries"),
        ContentJson = reader.GetText("content"),
        ShownAtUtc = reader.GetNullableUtcDateTime("shown_at_utc")
    };

    internal static ReportAdvice MapAdvice(SqliteDataReader reader) => new()
    {
        Id = reader.GetGuid("id"),
        ReportId = reader.GetGuid("report_id"),
        RuleId = reader.GetText("rule_id"),
        Direction = reader.GetEnum<LifeDirection>("direction"),
        Text = reader.GetText("text"),
        Feedback = reader.GetNullableEnum<AdviceFeedback>("feedback")
    };
}
