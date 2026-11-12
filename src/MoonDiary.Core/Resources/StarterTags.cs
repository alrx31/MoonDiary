using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Resources;

public static class StarterTags
{
    private static readonly Lazy<List<StarterTagDefinition>> Definitions = new(Load);

    public static IReadOnlyList<StarterTagDefinition> All => Definitions.Value;

    public static IReadOnlyList<Tag> CreateTags()
    {
        var orders = new Dictionary<CardType, int>();

        return All.Select(definition =>
        {
            orders.TryGetValue(definition.CardType, out var order);
            orders[definition.CardType] = order + 1;

            return new Tag
            {
                Id = Guid.NewGuid(),
                CardType = definition.CardType,
                Label = definition.Label,
                Icon = definition.Icon,
                Direction = definition.Direction,
                Order = order
            };
        }).ToList();
    }

    private static List<StarterTagDefinition> Load() =>
        ResourceStore.ReadJson<List<StarterTagDefinition>>("starter-tags.json");
}
