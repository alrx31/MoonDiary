using Microsoft.Data.Sqlite;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Data.Repositories;

public sealed class EntryRepository : IEntryRepository
{
    private const string Columns =
        "id, type, day_key, energy, positivity, done, achievement, feelings, checkin_text, " +
        "created_at_utc, updated_at_utc, timezone_offset, is_backfilled";

    private readonly SqliteConnectionFactory factory;

    public EntryRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<Entry?> GetByIdAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM entries WHERE id = $id", ("$id", SqlValues.FromGuid(id)))
            .FirstOrDefault();
    });

    public Task<Entry?> GetEveningAsync(DateOnly dayKey) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(
                Map,
                $"SELECT {Columns} FROM entries WHERE type = 'evening' AND day_key = $day",
                ("$day", SqlValues.FromDateOnly(dayKey)))
            .FirstOrDefault();
    });

    public Task<bool> HasEveningAsync(DateOnly dayKey) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.ScalarLong(
            "SELECT COUNT(*) FROM entries WHERE type = 'evening' AND day_key = $day",
            ("$day", SqlValues.FromDateOnly(dayKey))) > 0;
    });

    public Task<IReadOnlyList<Entry>> GetCheckinsAsync(DateOnly dayKey) => Task.Run<IReadOnlyList<Entry>>(() =>
    {
        using var c = factory.Open();

        return c.Query(
            Map,
            $"SELECT {Columns} FROM entries WHERE type = 'checkin' AND day_key = $day ORDER BY created_at_utc DESC",
            ("$day", SqlValues.FromDateOnly(dayKey)));
    });

    public Task<IReadOnlyList<Entry>> GetRangeAsync(DateOnly first, DateOnly last) => Task.Run<IReadOnlyList<Entry>>(() =>
    {
        using var c = factory.Open();

        return c.Query(
            Map,
            $"SELECT {Columns} FROM entries WHERE day_key >= $from AND day_key <= $to ORDER BY day_key, created_at_utc",
            ("$from", SqlValues.FromDateOnly(first)),
            ("$to", SqlValues.FromDateOnly(last)));
    });

    public Task<IReadOnlyList<Entry>> GetLatestEveningsAsync(int count, bool excludeBackfilled) =>
        Task.Run<IReadOnlyList<Entry>>(() =>
        {
            using var c = factory.Open();

            return c.Query(
                Map,
                $"SELECT {Columns} FROM entries WHERE type = 'evening' AND ($all = 1 OR is_backfilled = 0) " +
                "ORDER BY day_key DESC LIMIT $count",
                ("$all", excludeBackfilled ? 0 : 1),
                ("$count", count));
        });

    public Task<IReadOnlyList<Entry>> GetEveningsWithTextSinceAsync(DateOnly from) =>
        Task.Run<IReadOnlyList<Entry>>(() =>
        {
            using var c = factory.Open();

            return c.Query(
                Map,
                $"SELECT {Columns} FROM entries WHERE type = 'evening' AND day_key >= $from " +
                "AND (done IS NOT NULL OR achievement IS NOT NULL OR feelings IS NOT NULL) ORDER BY day_key",
                ("$from", SqlValues.FromDateOnly(from)));
        });

    public Task<IReadOnlyList<DateOnly>> GetEveningDaysAsync() => Task.Run<IReadOnlyList<DateOnly>>(() =>
    {
        using var c = factory.Open();

        return c.Query(
            reader => reader.GetDateOnly("day_key"),
            "SELECT day_key FROM entries WHERE type = 'evening' ORDER BY day_key DESC");
    });

    public Task SaveAsync(Entry entry) => Task.Run(() =>
    {
        MoodValue.EnsureValid(entry.Energy, nameof(entry));
        MoodValue.EnsureValid(entry.Positivity, nameof(entry));

        using var c = factory.Open();
        Insert(c, null, entry, upsert: true);
    });

    public Task DeleteAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();
        c.Execute("DELETE FROM entries WHERE id = $id", ("$id", SqlValues.FromGuid(id)));
    });

    public Task<IReadOnlyList<EntryTag>> GetEntryTagsAsync(IReadOnlyCollection<Guid> entryIds) =>
        Task.Run<IReadOnlyList<EntryTag>>(() =>
        {
            if (entryIds.Count == 0)
            {
                return [];
            }

            using var c = factory.Open();
            var result = new List<EntryTag>();

            foreach (var chunk in entryIds.Chunk(500))
            {
                var (sql, parameters) = SqliteCommandExtensions.InList("e", chunk);
                result.AddRange(c.Query(
                    MapTag,
                    $"SELECT entry_id, tag_id, card_type FROM entry_tags WHERE entry_id IN ({sql}) ORDER BY rowid",
                    parameters));
            }

            return result;
        });

    public Task ReplaceEntryTagsAsync(Guid entryId, IReadOnlyCollection<EntryTag> tags) => Task.Run(() =>
    {
        using var c = factory.Open();
        using var transaction = c.BeginTransaction();

        c.Execute(transaction, "DELETE FROM entry_tags WHERE entry_id = $id", ("$id", SqlValues.FromGuid(entryId)));

        foreach (var tag in tags)
        {
            InsertTag(c, transaction, tag);
        }

        transaction.Commit();
    });

    internal static void Insert(SqliteConnection c, SqliteTransaction? transaction, Entry entry, bool upsert)
    {
        var conflict = upsert
            ? " ON CONFLICT(id) DO UPDATE SET type = excluded.type, day_key = excluded.day_key, " +
              "energy = excluded.energy, positivity = excluded.positivity, done = excluded.done, " +
              "achievement = excluded.achievement, feelings = excluded.feelings, " +
              "checkin_text = excluded.checkin_text, created_at_utc = excluded.created_at_utc, " +
              "updated_at_utc = excluded.updated_at_utc, timezone_offset = excluded.timezone_offset, " +
              "is_backfilled = excluded.is_backfilled"
            : string.Empty;

        c.Execute(
            transaction,
            $"INSERT INTO entries ({Columns}) VALUES ($id, $type, $day, $energy, $positivity, $done, $achievement, " +
            $"$feelings, $checkin, $created, $updated, $offset, $backfilled){conflict}",
            ("$id", SqlValues.FromGuid(entry.Id)),
            ("$type", SqlValues.FromEnum(entry.Type)),
            ("$day", SqlValues.FromDateOnly(entry.DayKey)),
            ("$energy", entry.Energy),
            ("$positivity", entry.Positivity),
            ("$done", entry.Done),
            ("$achievement", entry.Achievement),
            ("$feelings", entry.Feelings),
            ("$checkin", entry.CheckinText),
            ("$created", SqlValues.FromUtcDateTime(entry.CreatedAtUtc)),
            ("$updated", SqlValues.FromUtcDateTime(entry.UpdatedAtUtc)),
            ("$offset", entry.TimezoneOffsetMinutes),
            ("$backfilled", entry.IsBackfilled ? 1 : 0));
    }

    internal static void InsertTag(SqliteConnection c, SqliteTransaction? transaction, EntryTag tag) =>
        c.Execute(
            transaction,
            "INSERT INTO entry_tags (entry_id, tag_id, card_type) VALUES ($entry, $tag, $card)",
            ("$entry", SqlValues.FromGuid(tag.EntryId)),
            ("$tag", SqlValues.FromGuid(tag.TagId)),
            ("$card", SqlValues.FromEnum(tag.CardType)));

    internal static Entry Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetGuid("id"),
        Type = reader.GetEnum<EntryType>("type"),
        DayKey = reader.GetDateOnly("day_key"),
        Energy = reader.GetInt("energy"),
        Positivity = reader.GetInt("positivity"),
        Done = reader.GetNullableString("done"),
        Achievement = reader.GetNullableString("achievement"),
        Feelings = reader.GetNullableString("feelings"),
        CheckinText = reader.GetNullableString("checkin_text"),
        CreatedAtUtc = reader.GetUtcDateTime("created_at_utc"),
        UpdatedAtUtc = reader.GetUtcDateTime("updated_at_utc"),
        TimezoneOffsetMinutes = reader.GetInt("timezone_offset"),
        IsBackfilled = reader.GetBool("is_backfilled")
    };

    internal static EntryTag MapTag(SqliteDataReader reader) => new(
        reader.GetGuid("entry_id"),
        reader.GetGuid("tag_id"),
        reader.GetEnum<CardType>("card_type"));
}
