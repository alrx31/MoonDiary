using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class EntryTagBuilder
{
    private EntryTag entryTag;

    private EntryTagBuilder(Guid entryId, Guid tagId, CardType cardType)
    {
        entryTag = new EntryTag(entryId, tagId, cardType);
    }

    public static EntryTagBuilder For(Guid entryId, Guid tagId, CardType cardType) => new(entryId, tagId, cardType);

    public EntryTagBuilder WithCardType(CardType cardType)
    {
        entryTag = entryTag with { CardType = cardType };

        return this;
    }

    public EntryTag Build() => entryTag;
}
