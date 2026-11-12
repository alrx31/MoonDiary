using MoonDiary.Core.Domain;

namespace MoonDiary.Presentation.Services;

public interface IThemeService
{
    void Apply(ThemeMode mode);
}
