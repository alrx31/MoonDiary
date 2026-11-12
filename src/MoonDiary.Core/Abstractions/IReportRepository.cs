using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Abstractions;

public interface IReportRepository
{
    Task<Report?> GetByIdAsync(Guid id);

    Task<Report?> GetByWeekStartAsync(DateOnly weekStart);

    Task<IReadOnlyList<Report>> GetAllAsync();

    Task<Report?> GetLatestAsync();

    Task<Report?> GetUnshownAsync();

    Task SaveAsync(Report report, IReadOnlyList<ReportAdvice> advice);

    Task MarkShownAsync(Guid id, DateTime shownAtUtc);

    Task<IReadOnlyList<ReportAdvice>> GetAdviceAsync(Guid reportId);

    Task SetAdviceFeedbackAsync(Guid adviceId, AdviceFeedback? feedback);

    Task<IReadOnlyList<string>> GetRuleIdsWithFeedbackSinceAsync(AdviceFeedback feedback, DateOnly minWeekStart);
}
