using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class TagBuilder
{
    private readonly Tag tag;

    private TagBuilder(CardType card, string label)
    {
        tag = new Tag
        {
            Id = Guid.NewGuid(),
            CardType = card,
            Label = label,
            Icon = "🌙",
            Direction = LifeDirection.None
        };
    }

    public static TagBuilder For(CardType card, string label) => new(card, label);

    public TagBuilder WithDirection(LifeDirection direction)
    {
        tag.Direction = direction;

        return this;
    }

    public TagBuilder WithOrder(int order)
    {
        tag.Order = order;

        return this;
    }

    public TagBuilder WithUseCount(int useCount)
    {
        tag.UseCount = useCount;

        return this;
    }

    public TagBuilder Archived()
    {
        tag.IsArchived = true;

        return this;
    }

    public TagBuilder Custom()
    {
        tag.IsCustom = true;

        return this;
    }

    public Tag Build() => tag;
}
