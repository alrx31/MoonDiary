namespace MoonDiary.Core.Domain;

public sealed class Draft
{
    public DateOnly DayKey { get; set; }

    public string ContentJson { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }
}
