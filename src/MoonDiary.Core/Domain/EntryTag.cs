namespace MoonDiary.Core.Domain;

public sealed record EntryTag(Guid EntryId, Guid TagId, CardType CardType);
