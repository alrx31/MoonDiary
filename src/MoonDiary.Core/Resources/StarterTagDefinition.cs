using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Resources;

public sealed record StarterTagDefinition(
    CardType CardType,
    string Label,
    string Icon,
    LifeDirection Direction);
