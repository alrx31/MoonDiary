using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class TestReportBuilder
{
    private readonly Report report;

    private TestReportBuilder(DateOnly weekStart)
    {
        report = new Report
        {
            Id = Guid.NewGuid(),
            WeekStart = weekStart,
            GeneratedAtUtc = BuilderTime.Utc(weekStart, new TimeOnly(9, 0)),
            Status = ReportStatus.Ok,
            DaysWithEntries = 7,
            ContentJson = "{}"
        };
    }

    public static TestReportBuilder For(DateOnly weekStart) => new(weekStart);

    public TestReportBuilder WithStatus(ReportStatus status)
    {
        report.Status = status;

        return this;
    }

    public TestReportBuilder WithDaysWithEntries(int days)
    {
        report.DaysWithEntries = days;

        return this;
    }

    public TestReportBuilder WithContent(string contentJson)
    {
        report.ContentJson = contentJson;

        return this;
    }

    public TestReportBuilder GeneratedAt(DateTime utc)
    {
        report.GeneratedAtUtc = utc;

        return this;
    }

    public TestReportBuilder ShownAt(DateTime utc)
    {
        report.ShownAtUtc = utc;

        return this;
    }

    public TestReportBuilder WithId(Guid id)
    {
        report.Id = id;

        return this;
    }

    public Report Build() => report;
}
