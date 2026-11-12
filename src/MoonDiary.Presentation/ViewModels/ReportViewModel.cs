using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Reports;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class ReportViewModel : ViewModelBase
{
    private readonly WeeklyReportService reports;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<string> summary = [];

    [ObservableProperty]
    private IReadOnlyList<string> achievements = [];

    [ObservableProperty]
    private IReadOnlyList<string> patterns = [];

    [ObservableProperty]
    private IReadOnlyList<ComparisonRowViewModel> comparison = [];

    [ObservableProperty]
    private IReadOnlyList<AdviceItemViewModel> advice = [];

    [ObservableProperty]
    private bool isInsufficient;

    [ObservableProperty]
    private bool isFull;

    [ObservableProperty]
    private bool hasComparison;

    [ObservableProperty]
    private bool hasAdvice;

    [ObservableProperty]
    private bool hasNoAdvice;

    public ReportViewModel(ViewModelContext context, WeeklyReportService reports)
        : base(context)
    {
        this.reports = reports;
    }

    public override async Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        var id = parameters.GetGuid(NavParams.ReportId);
        var report = id is null ? await reports.GetLatestAsync() : await reports.GetAsync(id.Value);

        if (report is null)
        {
            await Navigation.GoBackAsync();

            return;
        }

        Title = string.Format(
            CultureInfo.InvariantCulture,
            Strings.Report_Title,
            Ru.WeekRange(report.WeekStart, report.WeekEnd));
        Summary = report.Content.Summary;
        Achievements = report.Content.Achievements;
        Patterns = report.Content.Patterns;
        IsInsufficient = report.Status == ReportStatus.InsufficientData;
        IsFull = !IsInsufficient;
        Comparison = report.Content.Comparison is { } weeks
            ?
            [
                new ComparisonRowViewModel(Strings.Common_Energy, weeks.Energy, weeks.EnergyDelta),
                new ComparisonRowViewModel(Strings.Common_Positivity, weeks.Positivity, weeks.PositivityDelta)
            ]
            : [];
        HasComparison = Comparison.Count > 0;
        Advice = report.Advice.Select((item, index) => new AdviceItemViewModel(item, index)).ToList();
        HasAdvice = Advice.Count > 0;
        HasNoAdvice = IsFull && !HasAdvice;

        if (!report.WasShown)
        {
            await reports.MarkShownAsync(report.Id);
        }
    }

    [RelayCommand]
    private Task MarkUsefulAsync(AdviceItemViewModel item) => SetFeedbackAsync(item, AdviceFeedback.Useful);

    [RelayCommand]
    private Task MarkNotRelevantAsync(AdviceItemViewModel item) => SetFeedbackAsync(item, AdviceFeedback.NotRelevant);

    [RelayCommand]
    private Task CloseAsync() => Navigation.GoBackAsync();

    private async Task SetFeedbackAsync(AdviceItemViewModel item, AdviceFeedback feedback)
    {
        AdviceFeedback? value = item.Feedback == feedback ? null : feedback;

        if (await RunAsync(() => reports.SetFeedbackAsync(item.Id, value)))
        {
            item.Feedback = value;
        }
    }
}
