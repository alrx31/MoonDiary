using MoonDiary.Core.Analytics;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Reports;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Reports;

public class ReportTests
{
    private static readonly DateOnly Week = Dates.D(2026, 11, 9);

    private static readonly DateTime Now = new(2026, 11, 16, 6, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Build_InsufficientData_HasOnlySummary()
    {
        var input = AnalyticsInputFactory.Week(Week, (3, 3)).Build();

        var (report, advice) = ReportFactory.Build(input, AnalyticsOptions.Default, [], Now);
        var content = ReportContent.Deserialize(report.ContentJson);

        Assert.Equal(ReportStatus.InsufficientData, report.Status);
        Assert.Equal(1, report.DaysWithEntries);
        Assert.True(content.Insufficient);
        Assert.Empty(content.Achievements);
        Assert.Empty(content.Patterns);
        Assert.Empty(advice);
    }

    [Fact]
    public void Build_FullWeek_HasAllPartsAndAtMostThreeAdvice()
    {
        var input = AnalyticsInputFactory.Week(Week, (2, 3), (2, 3), (1, 2), (4, 4), (3, 3))
            .WithPrevious((3, 3), (3, 3), (3, 3))
            .WithAchievement(Week, "сдал задачу")
            .Build();

        var (report, advice) = ReportFactory.Build(input, AnalyticsOptions.Default, [], Now);
        var content = ReportContent.Deserialize(report.ContentJson);

        Assert.Equal(ReportStatus.Ok, report.Status);
        Assert.NotEmpty(content.Summary);
        Assert.NotEmpty(content.Achievements);
        Assert.NotEmpty(content.Patterns);
        Assert.NotNull(content.Comparison);
        Assert.InRange(advice.Count, 1, 3);
        Assert.All(advice, item => Assert.Equal(report.Id, item.ReportId));
    }

    [Fact]
    public async Task Ensure_MondayBoundary_PicksCompletedWeek()
    {
        using var early = TestApp.At(2026, 11, 16, 3, 59);
        using var late = TestApp.At(2026, 11, 16, 4, 0);
        await early.EveningAsync(Dates.D(2026, 11, 5), 3, 3);
        await late.EveningAsync(Dates.D(2026, 11, 12), 3, 3);

        Assert.Equal(Dates.D(2026, 11, 2), (await early.ReportService.EnsureLastWeekReportAsync())!.WeekStart);
        Assert.Equal(Dates.D(2026, 11, 9), (await late.ReportService.EnsureLastWeekReportAsync())!.WeekStart);
    }

    [Fact]
    public async Task Ensure_SecondCall_ReturnsNullAndKeepsReport()
    {
        using var app = TestApp.At(2026, 11, 10, 20, 0);
        await app.EveningAsync(Dates.D(2026, 11, 10), 2, 2);
        await app.EveningAsync(Dates.D(2026, 11, 9), 2, 2);
        await app.EveningAsync(Dates.D(2026, 11, 8), 2, 2);
        app.SetLocal(2026, 11, 16, 9, 0);

        var first = await app.ReportService.EnsureLastWeekReportAsync();
        await app.EveningAsync(Dates.D(2026, 11, 11), 5, 5);
        var second = await app.ReportService.EnsureLastWeekReportAsync();
        var stored = await app.ReportService.GetAsync(first!.Id);

        Assert.Null(second);
        Assert.Equal(2, stored!.DaysWithEntries);
        Assert.Equal(ReportStatus.InsufficientData, stored.Status);
    }

    [Fact]
    public async Task Ensure_AfterMissedWeeks_BuildsOnlyLastWeek()
    {
        using var app = TestApp.At(2026, 11, 30, 9, 0);
        await app.EveningAsync(Dates.D(2026, 11, 10), 3, 3);
        await app.EveningAsync(Dates.D(2026, 11, 25), 3, 3);

        await app.ReportService.EnsureLastWeekReportAsync();

        Assert.Equal(Dates.D(2026, 11, 23), Assert.Single(await app.ReportService.GetHistoryAsync()).WeekStart);
    }

    [Fact]
    public async Task Ensure_WeekWithoutEntries_BuildsNothing()
    {
        using var app = TestApp.At(2026, 11, 16, 9, 0);

        Assert.Null(await app.ReportService.EnsureLastWeekReportAsync());
        Assert.Empty(await app.ReportService.GetHistoryAsync());
    }

    [Fact]
    public async Task MarkShown_ClearsUnshownReport()
    {
        using var app = TestApp.At(2026, 11, 16, 9, 0);
        await app.EveningAsync(Dates.D(2026, 11, 12), 3, 3);
        var report = await app.ReportService.EnsureLastWeekReportAsync();

        Assert.Equal(report!.Id, await app.ReportService.GetUnshownIdAsync());
        await app.ReportService.MarkShownAsync(report.Id);

        Assert.Null(await app.ReportService.GetUnshownIdAsync());
        Assert.True((await app.ReportService.GetLatestAsync())!.WasShown);
    }

    [Fact]
    public async Task NotRelevantFeedback_ExcludesRuleFromNextReport()
    {
        using var app = TestApp.At(2026, 11, 15, 20, 0);

        for (var offset = 0; offset < 14; offset++)
        {
            await app.EveningAsync(Dates.D(2026, 11, 15).AddDays(-offset), 2, 3);
        }

        app.SetLocal(2026, 11, 16, 9, 0);
        var first = await app.ReportService.EnsureLastWeekReportAsync();
        var sleep = Assert.Single(first!.Advice, advice => advice.RuleId == "AR-30");
        await app.ReportService.SetFeedbackAsync(sleep.Id, AdviceFeedback.NotRelevant);
        app.SetLocal(2026, 11, 22, 20, 0);

        for (var offset = 0; offset < 7; offset++)
        {
            await app.EveningAsync(Dates.D(2026, 11, 16).AddDays(offset), 2, 3);
        }

        app.SetLocal(2026, 11, 23, 9, 0);
        var second = await app.ReportService.EnsureLastWeekReportAsync();

        Assert.DoesNotContain(second!.Advice, advice => advice.RuleId == "AR-30");
    }
}
