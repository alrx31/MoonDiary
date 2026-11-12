using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Transfer;

namespace MoonDiary.Data.Repositories;

public sealed class DataTransferRepository : IDataTransferRepository
{
    private static readonly string[] Tables =
        ["entry_tags", "report_advice", "entries", "tags", "reports", "drafts", "letters", "settings"];

    private readonly SqliteConnectionFactory factory;

    public DataTransferRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<ExportBundle> ReadAllAsync() => Task.Run(() =>
    {
        using var c = factory.Open();

        return new ExportBundle
        {
            Entries = c.Query(
                EntryRepository.Map,
                "SELECT * FROM entries ORDER BY day_key, created_at_utc"),
            Tags = c.Query(TagRepository.Map, "SELECT * FROM tags ORDER BY card_type, sort_order"),
            EntryTags = c.Query(EntryRepository.MapTag, "SELECT * FROM entry_tags ORDER BY rowid"),
            Reports = c.Query(ReportRepository.Map, "SELECT * FROM reports ORDER BY week_start"),
            Advice = c.Query(ReportRepository.MapAdvice, "SELECT * FROM report_advice ORDER BY report_id, sort_order"),
            Drafts = c.Query(DraftRepository.Map, "SELECT * FROM drafts ORDER BY day_key"),
            Letters = c.Query(LetterRepository.Map, "SELECT * FROM letters ORDER BY open_date, created_at_utc"),
            Settings = SettingsRepository.Read(c)
        };
    });

    public Task ReplaceAllAsync(ExportBundle bundle) => Task.Run(() =>
    {
        using var c = factory.Open();
        using var transaction = c.BeginTransaction();

        foreach (var table in Tables)
        {
            c.Execute(transaction, $"DELETE FROM {table}");
        }

        foreach (var tag in bundle.Tags)
        {
            TagRepository.Insert(c, transaction, tag, upsert: false);
        }

        foreach (var entry in bundle.Entries)
        {
            EntryRepository.Insert(c, transaction, entry, upsert: false);
        }

        foreach (var entryTag in bundle.EntryTags)
        {
            EntryRepository.InsertTag(c, transaction, entryTag);
        }

        foreach (var report in bundle.Reports)
        {
            ReportRepository.Insert(c, transaction, report);
        }

        foreach (var group in bundle.Advice.GroupBy(advice => advice.ReportId))
        {
            var order = 0;

            foreach (var advice in group)
            {
                ReportRepository.InsertAdvice(c, transaction, advice, order++);
            }
        }

        foreach (var draft in bundle.Drafts)
        {
            DraftRepository.Insert(c, transaction, draft);
        }

        foreach (var letter in bundle.Letters)
        {
            LetterRepository.Insert(c, transaction, letter, upsert: false);
        }

        SettingsRepository.Write(c, transaction, bundle.Settings);
        transaction.Commit();
    });
}
