using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Insights;
using MoonDiary.Core.Reports;
using MoonDiary.Core.Time;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class AnalyticsViewModel : ViewModelBase
{
    private readonly WeeklyReportService reports;
    private readonly InsightsService insights;
    private readonly DayClock clock;
    private Guid? latestReportId;
    private int year;
    private int month;

    [ObservableProperty]
    private bool hasReport;

    [ObservableProperty]
    private string reportTitle = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<string> reportSummary = [];

    [ObservableProperty]
    private IReadOnlyList<DynamicsPoint> dynamics = [];

    [ObservableProperty]
    private bool hasDynamics;

    [ObservableProperty]
    private string achievementsMonth = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<AchievementRowViewModel> achievements = [];

    [ObservableProperty]
    private bool hasAchievements;

    [ObservableProperty]
    private bool canGoNextMonth;

    [ObservableProperty]
    private string streak = string.Empty;

    public AnalyticsViewModel(
        ViewModelContext context,
        WeeklyReportService reports,
        InsightsService insights,
        DayClock clock)
        : base(context)
    {
        this.reports = reports;
        this.insights = insights;
        this.clock = clock;

        var today = clock.CurrentDayKey;
        year = today.Year;
        month = today.Month;
    }

    public override async Task RefreshAsync()
    {
        try
        {
            var latest = await reports.GetLatestAsync();

            latestReportId = latest?.Id;
            HasReport = latest is not null;
            ReportTitle = latest is null ? string.Empty : Ru.WeekRange(latest.WeekStart, latest.WeekEnd);
            ReportSummary = latest?.Content.Summary ?? [];
            Dynamics = await insights.GetDynamicsAsync();
            HasDynamics = Dynamics.Any(point => point.Mood is not null);
            Streak = StreakText(await insights.GetStreakAsync());
            await LoadAchievementsAsync();
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(AnalyticsViewModel), exception);
        }
    }

    public static string StreakText(int days) =>
        days == 0 ? "Серии пока нет" : $"{Ru.Count(days, "день", "дня", "дней")} подряд";

    [RelayCommand]
    private Task OpenReportAsync() =>
        latestReportId is { } id
            ? Navigation.PushAsync(Routes.Report, NavParams.Of((NavParams.ReportId, id)))
            : Task.CompletedTask;

    [RelayCommand]
    private Task OpenReportsAsync() => Navigation.PushAsync(Routes.Reports);

    [RelayCommand]
    private Task PreviousMonthAsync() => ShiftMonthAsync(-1);

    [RelayCommand]
    private Task NextMonthAsync() => CanGoNextMonth ? ShiftMonthAsync(1) : Task.CompletedTask;

    private Task ShiftMonthAsync(int delta)
    {
        var first = new DateOnly(year, month, 1).AddMonths(delta);
        year = first.Year;
        month = first.Month;

        return LoadAchievementsAsync();
    }

    private async Task LoadAchievementsAsync()
    {
        var today = clock.CurrentDayKey;

        AchievementsMonth = Ru.MonthTitle(year, month);
        Achievements = (await insights.GetMonthAchievementsAsync(year, month))
            .Select(item => new AchievementRowViewModel(item))
            .ToList();
        HasAchievements = Achievements.Count > 0;
        CanGoNextMonth = new DateOnly(year, month, 1) < new DateOnly(today.Year, today.Month, 1);
    }
}
