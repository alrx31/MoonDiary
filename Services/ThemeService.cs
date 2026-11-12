using MoonDiary.Core.Domain;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(ThemeMode mode) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is not null)
            {
                Application.Current.UserAppTheme = mode switch
                {
                    ThemeMode.Light => AppTheme.Light,
                    ThemeMode.Dark => AppTheme.Dark,
                    _ => AppTheme.Unspecified
                };
            }
        });
}
