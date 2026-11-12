using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Resources;

namespace MoonDiary.Core.Tags;

public sealed class TagService
{
    public const int MaxSuggestions = 10;

    private readonly ITagRepository tags;

    public TagService(ITagRepository tags)
    {
        this.tags = tags;
    }

    public async Task<IReadOnlyList<Tag>> GetSuggestionsForCardAsync(CardType cardType) =>
        (await tags.GetActiveAsync(cardType)).Take(MaxSuggestions).ToList();

    public async Task<IReadOnlyList<Tag>> GetManageListAsync(CardType cardType) =>
        (await tags.GetActiveAsync(cardType)).OrderBy(tag => tag.Order).ToList();

    public async Task<Tag> AddAsync(CardType cardType, string label, string icon, LifeDirection direction)
    {
        var normalized = NormalizeLabel(label);
        await EnsureUniqueAsync(cardType, normalized, null);

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            CardType = cardType,
            Label = normalized,
            Icon = string.IsNullOrWhiteSpace(icon) ? "•" : icon.Trim(),
            Direction = direction,
            Order = await tags.GetNextOrderAsync(cardType),
            IsCustom = true
        };

        await tags.SaveAsync(tag);

        return tag;
    }

    public async Task RenameAsync(Guid id, string label)
    {
        var tag = await GetAsync(id);
        var normalized = NormalizeLabel(label);
        await EnsureUniqueAsync(tag.CardType, normalized, id);

        tag.Label = normalized;
        await tags.SaveAsync(tag);
    }

    public async Task ChangeDirectionAsync(Guid id, LifeDirection direction)
    {
        var tag = await GetAsync(id);
        tag.Direction = direction;
        await tags.SaveAsync(tag);
    }

    public Task ArchiveAsync(Guid id) => tags.ArchiveAsync(id);

    public Task ReorderAsync(CardType cardType, IReadOnlyList<Guid> orderedIds) =>
        tags.ReorderAsync(cardType, orderedIds);

    public async Task MoveAsync(Guid id, int delta)
    {
        var tag = await GetAsync(id);
        var ordered = (await GetManageListAsync(tag.CardType)).Select(item => item.Id).ToList();
        var index = ordered.IndexOf(id);
        var target = index + delta;

        if (index < 0 || target < 0 || target >= ordered.Count)
        {
            return;
        }

        ordered.RemoveAt(index);
        ordered.Insert(target, id);
        await tags.ReorderAsync(tag.CardType, ordered);
    }

    public Task<bool> SeedStarterTagsAsync() => tags.SeedIfEmptyAsync(StarterTags.CreateTags());

    private async Task<Tag> GetAsync(Guid id) =>
        await tags.GetByIdAsync(id) ?? throw new DomainException("Блок не найден");

    private async Task EnsureUniqueAsync(CardType cardType, string label, Guid? exceptId)
    {
        var active = await tags.GetActiveAsync(cardType);

        if (active.Any(tag => tag.Id != exceptId && string.Equals(tag.Label, label, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DomainException("Такой блок уже есть");
        }
    }

    private static string NormalizeLabel(string label)
    {
        var words = (label ?? string.Empty)
            .Split([' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            throw new DomainException("Введите подпись блока");
        }

        if (words.Length > TextLimits.TagLabelMaxWords)
        {
            throw new DomainException($"В подписи не больше {TextLimits.TagLabelMaxWords} слов");
        }

        return string.Join(' ', words);
    }
}
