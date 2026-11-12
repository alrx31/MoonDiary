using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Abstractions;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetActiveAsync(CardType cardType);

    Task<IReadOnlyList<Tag>> GetAllAsync();

    Task<IReadOnlyList<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids);

    Task<Tag?> GetByIdAsync(Guid id);

    Task SaveAsync(Tag tag);

    Task ArchiveAsync(Guid id);

    Task ReorderAsync(CardType cardType, IReadOnlyList<Guid> orderedIds);

    Task IncrementUseCountAsync(IReadOnlyCollection<Guid> ids);

    Task<bool> SeedIfEmptyAsync(IReadOnlyList<Tag> tags);

    Task<int> GetNextOrderAsync(CardType cardType);
}
