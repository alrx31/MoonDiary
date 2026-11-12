using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Abstractions;

public interface IDraftRepository
{
    Task<Draft?> GetAsync(DateOnly dayKey);

    Task<IReadOnlyList<Draft>> GetAllAsync();

    Task<int> CountAsync();

    Task SaveAsync(Draft draft);

    Task DeleteAsync(DateOnly dayKey);
}
