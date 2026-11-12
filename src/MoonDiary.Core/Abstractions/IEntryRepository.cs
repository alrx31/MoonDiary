using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Abstractions;

public interface IEntryRepository
{
    Task<Entry?> GetByIdAsync(Guid id);

    Task<Entry?> GetEveningAsync(DateOnly dayKey);

    Task<bool> HasEveningAsync(DateOnly dayKey);

    Task<IReadOnlyList<Entry>> GetCheckinsAsync(DateOnly dayKey);

    Task<IReadOnlyList<Entry>> GetRangeAsync(DateOnly first, DateOnly last);

    Task<IReadOnlyList<Entry>> GetLatestEveningsAsync(int count, bool excludeBackfilled);

    Task<IReadOnlyList<Entry>> GetEveningsWithTextSinceAsync(DateOnly from);

    Task<IReadOnlyList<DateOnly>> GetEveningDaysAsync();

    Task SaveAsync(Entry entry);

    Task DeleteAsync(Guid id);

    Task<IReadOnlyList<EntryTag>> GetEntryTagsAsync(IReadOnlyCollection<Guid> entryIds);

    Task ReplaceEntryTagsAsync(Guid entryId, IReadOnlyCollection<EntryTag> tags);
}
