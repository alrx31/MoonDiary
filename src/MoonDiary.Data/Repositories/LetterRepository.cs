using Microsoft.Data.Sqlite;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Data.Repositories;

public sealed class LetterRepository : ILetterRepository
{
    private const string Columns = "id, text, created_at_utc, open_date, opened_at_utc, notified";

    private readonly SqliteConnectionFactory factory;

    public LetterRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<IReadOnlyList<Letter>> GetAllAsync() => Task.Run<IReadOnlyList<Letter>>(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM letters ORDER BY open_date, created_at_utc");
    });

    public Task<Letter?> GetByIdAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM letters WHERE id = $id", ("$id", SqlValues.FromGuid(id)))
            .FirstOrDefault();
    });

    public Task SaveAsync(Letter letter) => Task.Run(() =>
    {
        using var c = factory.Open();
        Insert(c, null, letter, upsert: true);
    });

    public Task DeleteAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();
        c.Execute("DELETE FROM letters WHERE id = $id", ("$id", SqlValues.FromGuid(id)));
    });

    internal static void Insert(SqliteConnection c, SqliteTransaction? transaction, Letter letter, bool upsert)
    {
        var conflict = upsert
            ? " ON CONFLICT(id) DO UPDATE SET text = excluded.text, created_at_utc = excluded.created_at_utc, " +
              "open_date = excluded.open_date, opened_at_utc = excluded.opened_at_utc, notified = excluded.notified"
            : string.Empty;

        c.Execute(
            transaction,
            $"INSERT INTO letters ({Columns}) VALUES ($id, $text, $created, $open, $opened, $notified){conflict}",
            ("$id", SqlValues.FromGuid(letter.Id)),
            ("$text", letter.Text),
            ("$created", SqlValues.FromUtcDateTime(letter.CreatedAtUtc)),
            ("$open", SqlValues.FromDateOnly(letter.OpenDate)),
            ("$opened", letter.OpenedAtUtc is null ? null : SqlValues.FromUtcDateTime(letter.OpenedAtUtc.Value)),
            ("$notified", letter.Notified ? 1 : 0));
    }

    internal static Letter Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetGuid("id"),
        Text = reader.GetText("text"),
        CreatedAtUtc = reader.GetUtcDateTime("created_at_utc"),
        OpenDate = reader.GetDateOnly("open_date"),
        OpenedAtUtc = reader.GetNullableUtcDateTime("opened_at_utc"),
        Notified = reader.GetBool("notified")
    };
}
