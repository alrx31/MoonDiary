namespace MoonDiary.Core.Domain;

public sealed class Tag
{
    public Guid Id { get; set; }

    public CardType CardType { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public LifeDirection Direction { get; set; }

    public string? Color { get; set; }

    public int Order { get; set; }

    public int UseCount { get; set; }

    public bool IsCustom { get; set; }

    public bool IsArchived { get; set; }
}
