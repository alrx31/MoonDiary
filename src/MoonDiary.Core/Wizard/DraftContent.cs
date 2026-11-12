using System.Text.Json;
using MoonDiary.Core.Resources;

namespace MoonDiary.Core.Wizard;

public sealed record DraftContent
{
    public int? Energy { get; init; }

    public int? Positivity { get; init; }

    public string? Done { get; init; }

    public string? Achievement { get; init; }

    public string? Feelings { get; init; }

    public List<Guid> DoneTagIds { get; init; } = [];

    public List<Guid> AchievementTagIds { get; init; } = [];

    public List<Guid> FeelingsTagIds { get; init; } = [];

    public int CurrentStep { get; init; } = 1;

    public bool HasMood => Energy is not null || Positivity is not null;

    public int FilledTextCards =>
        new[] { Done, Achievement, Feelings }.Count(text => !string.IsNullOrWhiteSpace(text));

    public int FilledCardsCount => (HasMood ? 1 : 0) + FilledTextCards;

    public static string Serialize(DraftContent content) =>
        JsonSerializer.Serialize(content, ResourceStore.JsonOptions);

    public static DraftContent Deserialize(string json) =>
        JsonSerializer.Deserialize<DraftContent>(json, ResourceStore.JsonOptions)
        ?? throw new JsonException("Пустой черновик.");
}
