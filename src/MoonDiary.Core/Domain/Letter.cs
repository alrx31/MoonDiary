namespace MoonDiary.Core.Domain;

public sealed class Letter
{
    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateOnly OpenDate { get; set; }

    public DateTime? OpenedAtUtc { get; set; }

    public bool Notified { get; set; }
}
