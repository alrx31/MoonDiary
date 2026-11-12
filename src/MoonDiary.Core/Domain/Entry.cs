namespace MoonDiary.Core.Domain;

public sealed class Entry
{
    public Guid Id { get; set; }

    public EntryType Type { get; set; }

    public DateOnly DayKey { get; set; }

    public int Energy { get; set; }

    public int Positivity { get; set; }

    public string? Done { get; set; }

    public string? Achievement { get; set; }

    public string? Feelings { get; set; }

    public string? CheckinText { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public int TimezoneOffsetMinutes { get; set; }

    public bool IsBackfilled { get; set; }
}
