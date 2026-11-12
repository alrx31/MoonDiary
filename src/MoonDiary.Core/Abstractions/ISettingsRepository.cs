using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Abstractions;

public interface ISettingsRepository
{
    Task<AppSettings> GetAsync();

    Task SaveAsync(AppSettings settings);
}
