using Microsoft.Data.Sqlite;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Data.Repositories;

public sealed class DraftRepository : IDraftRepository
{
    private readonly SqliteConnectionFactory factory;

    public DraftRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<Draft?> GetAsync(DateOnly dayKey) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(
                Map,
                "SELECT day_key, content, updated_at_utc FROM drafts WHERE day_key = $day",
                ("$day", SqlValues.FromDateOnly(dayKey)))
            .FirstOrDefault();
    });

    public Task<IReadOnlyList<Draft>> GetAllAsync() => Task.Run<IReadOnlyList<Draft>>(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, "SELECT day_key, content, updated_at_utc FROM drafts ORDER BY day_key DESC");
    });

    public Task<int> CountAsync() => Task.Run(() =>
    {
        using var c = factory.Open();

        return (int)c.ScalarLong("SELECT COUNT(*) FROM drafts");
    });

    public Task SaveAsync(Draft draft) => Task.Run(() =>
    {
        using var c = factory.Open();
        Insert(c, null, draft);
    });

    public Task DeleteAsync(DateOnly dayKey) => Task.Run(() =>
    {
        using var c = factory.Open();
        c.Execute("DELETE FROM drafts WHERE day_key = $day", ("$day", SqlValues.FromDateOnly(dayKey)));
    });

    internal static void Insert(SqliteConnection c, SqliteTransaction? transaction, Draft draft) =>
        c.Execute(
            transaction,
            "INSERT INTO drafts (day_key, content, updated_at_utc) VALUES ($day, $content, $updated) " +
            "ON CONFLICT(day_key) DO UPDATE SET content = excluded.content, updated_at_utc = excluded.updated_at_utc",
            ("$day", SqlValues.FromDateOnly(draft.DayKey)),
            ("$content", draft.ContentJson),
            ("$updated", SqlValues.FromUtcDateTime(draft.UpdatedAtUtc)));

    internal static Draft Map(SqliteDataReader reader) => new()
    {
        DayKey = reader.GetDateOnly("day_key"),
        ContentJson = reader.GetText("content"),
        UpdatedAtUtc = reader.GetUtcDateTime("updated_at_utc")
    };
}
