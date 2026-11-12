namespace MoonDiary.Core.Domain;

public sealed class ReportAdvice
{
    public Guid Id { get; set; }

    public Guid ReportId { get; set; }

    public string RuleId { get; set; } = string.Empty;

    public LifeDirection Direction { get; set; }

    public string Text { get; set; } = string.Empty;

    public AdviceFeedback? Feedback { get; set; }
}
