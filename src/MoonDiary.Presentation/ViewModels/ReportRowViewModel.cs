using MoonDiary.Core.Reports;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class ReportRowViewModel
{
    public ReportRowViewModel(ReportView report)
    {
        Id = report.Id;
        Text = $"{Ru.WeekRange(report.WeekStart, report.WeekEnd)} · " +
               $"{Ru.Count(report.DaysWithEntries, "день", "дня", "дней")} записей";
    }

    public Guid Id { get; }

    public string Text { get; }
}
