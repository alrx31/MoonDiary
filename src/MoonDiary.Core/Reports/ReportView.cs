using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Reports;

public sealed record ReportView(
    Guid Id,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    ReportStatus Status,
    int DaysWithEntries,
    ReportContent Content,
    IReadOnlyList<ReportAdvice> Advice,
    bool WasShown);
