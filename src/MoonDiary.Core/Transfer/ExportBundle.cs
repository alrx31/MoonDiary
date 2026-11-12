using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Transfer;

public sealed class ExportBundle
{
    public int SchemaVersion { get; set; }

    public DateTime ExportedAtUtc { get; set; }

    public List<Entry> Entries { get; set; } = [];

    public List<Tag> Tags { get; set; } = [];

    public List<EntryTag> EntryTags { get; set; } = [];

    public List<Report> Reports { get; set; } = [];

    public List<ReportAdvice> Advice { get; set; } = [];

    public List<Draft> Drafts { get; set; } = [];

    public List<Letter> Letters { get; set; } = [];

    public AppSettings Settings { get; set; } = new();
}
