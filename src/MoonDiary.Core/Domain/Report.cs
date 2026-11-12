namespace MoonDiary.Core.Domain;

public sealed class Report
{
    public Guid Id { get; set; }

    public DateOnly WeekStart { get; set; }

    public DateTime GeneratedAtUtc { get; set; }

    public ReportStatus Status { get; set; }

    public int DaysWithEntries { get; set; }

    public string ContentJson { get; set; } = string.Empty;

    public DateTime? ShownAtUtc { get; set; }
}
