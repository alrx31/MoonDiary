using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Abstractions;

public interface ILetterRepository
{
    Task<IReadOnlyList<Letter>> GetAllAsync();

    Task<Letter?> GetByIdAsync(Guid id);

    Task SaveAsync(Letter letter);

    Task DeleteAsync(Guid id);
}
