using Microsoft.Data.Sqlite;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Data.Repositories;

public sealed class TagRepository : ITagRepository
{
    private const string Columns =
        "id, card_type, label, icon, direction, color, sort_order, use_count, is_custom, is_archived";

    private readonly SqliteConnectionFactory factory;

    public TagRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<IReadOnlyList<Tag>> GetActiveAsync(CardType cardType) => Task.Run<IReadOnlyList<Tag>>(() =>
    {
        using var c = factory.Open();

        return c.Query(
            Map,
            $"SELECT {Columns} FROM tags WHERE card_type = $card AND is_archived = 0 " +
            "ORDER BY use_count DESC, sort_order ASC",
            ("$card", SqlValues.FromEnum(cardType)));
    });

    public Task<IReadOnlyList<Tag>> GetAllAsync() => Task.Run<IReadOnlyList<Tag>>(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM tags ORDER BY card_type, sort_order");
    });

    public Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids) => Task.Run<IReadOnlyList<Tag>>(() =>
    {
        if (ids.Count == 0)
        {
            return [];
        }

        using var c = factory.Open();
        var result = new List<Tag>();

        foreach (var chunk in ids.Distinct().Chunk(500))
        {
            var (sql, parameters) = SqliteCommandExtensions.InList("t", chunk);
            result.AddRange(c.Query(Map, $"SELECT {Columns} FROM tags WHERE id IN ({sql})", parameters));
        }

        return result;
    });

    public Task<Tag?> GetByIdAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();

        return c.Query(Map, $"SELECT {Columns} FROM tags WHERE id = $id", ("$id", SqlValues.FromGuid(id)))
            .FirstOrDefault();
    });

    public Task SaveAsync(Tag tag) => Task.Run(() =>
    {
        tag.Label = NormalizeLabel(tag.Label);

        using var c = factory.Open();
        Insert(c, null, tag, upsert: true);
    });

    public Task ArchiveAsync(Guid id) => Task.Run(() =>
    {
        using var c = factory.Open();
        c.Execute("UPDATE tags SET is_archived = 1 WHERE id = $id", ("$id", SqlValues.FromGuid(id)));
    });

    public Task ReorderAsync(CardType cardType, IReadOnlyList<Guid> orderedIds) => Task.Run(() =>
    {
        using var c = factory.Open();
        using var transaction = c.BeginTransaction();

        for (var index = 0; index < orderedIds.Count; index++)
        {
            c.Execute(
                transaction,
                "UPDATE tags SET sort_order = $order WHERE id = $id AND card_type = $card",
                ("$order", index),
                ("$id", SqlValues.FromGuid(orderedIds[index])),
                ("$card", SqlValues.FromEnum(cardType)));
        }

        transaction.Commit();
    });

    public Task IncrementUseCountAsync(IReadOnlyCollection<Guid> ids) => Task.Run(() =>
    {
        if (ids.Count == 0)
        {
            return;
        }

        using var c = factory.Open();
        using var transaction = c.BeginTransaction();

        foreach (var id in ids)
        {
            c.Execute(
                transaction,
                "UPDATE tags SET use_count = use_count + 1 WHERE id = $id",
                ("$id", SqlValues.FromGuid(id)));
        }

        transaction.Commit();
    });

    public Task<bool> SeedIfEmptyAsync(IReadOnlyList<Tag> tags) => Task.Run(() =>
    {
        using var c = factory.Open();
        using var transaction = c.BeginTransaction();

        using (var command = c.Command(transaction, "SELECT COUNT(*) FROM tags"))
        {
            if (Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture) > 0)
            {
                return false;
            }
        }

        foreach (var tag in tags)
        {
            Insert(c, transaction, tag, upsert: false);
        }

        transaction.Commit();

        return true;
    });

    public Task<int> GetNextOrderAsync(CardType cardType) => Task.Run(() =>
    {
        using var c = factory.Open();

        return (int)c.ScalarLong(
            "SELECT COALESCE(MAX(sort_order) + 1, 0) FROM tags WHERE card_type = $card",
            ("$card", SqlValues.FromEnum(cardType)));
    });

    public static string NormalizeLabel(string? label)
    {
        var trimmed = (label ?? string.Empty).Trim();
        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            throw new ArgumentException("Подпись блока не может быть пустой.", nameof(label));
        }

        if (words.Length > TextLimits.TagLabelMaxWords)
        {
            throw new ArgumentException(
                $"В подписи блока не больше {TextLimits.TagLabelMaxWords} слов.",
                nameof(label));
        }

        return string.Join(' ', words);
    }

    internal static void Insert(SqliteConnection c, SqliteTransaction? transaction, Tag tag, bool upsert)
    {
        var conflict = upsert
            ? " ON CONFLICT(id) DO UPDATE SET card_type = excluded.card_type, label = excluded.label, " +
              "icon = excluded.icon, direction = excluded.direction, color = excluded.color, " +
              "sort_order = excluded.sort_order, use_count = excluded.use_count, " +
              "is_custom = excluded.is_custom, is_archived = excluded.is_archived"
            : string.Empty;

        c.Execute(
            transaction,
            $"INSERT INTO tags ({Columns}) VALUES ($id, $card, $label, $icon, $direction, $color, $order, $count, " +
            $"$custom, $archived){conflict}",
            ("$id", SqlValues.FromGuid(tag.Id)),
            ("$card", SqlValues.FromEnum(tag.CardType)),
            ("$label", tag.Label),
            ("$icon", tag.Icon),
            ("$direction", SqlValues.FromEnum(tag.Direction)),
            ("$color", tag.Color),
            ("$order", tag.Order),
            ("$count", tag.UseCount),
            ("$custom", tag.IsCustom ? 1 : 0),
            ("$archived", tag.IsArchived ? 1 : 0));
    }

    internal static Tag Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetGuid("id"),
        CardType = reader.GetEnum<CardType>("card_type"),
        Label = reader.GetText("label"),
        Icon = reader.GetText("icon"),
        Direction = reader.GetEnum<LifeDirection>("direction"),
        Color = reader.GetNullableString("color"),
        Order = reader.GetInt("sort_order"),
        UseCount = reader.GetInt("use_count"),
        IsCustom = reader.GetBool("is_custom"),
        IsArchived = reader.GetBool("is_archived")
    };
}
