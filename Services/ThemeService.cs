using MoonDiary.Core.Domain;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(ThemeMode mode) =>
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is null)
            {
                return;
            }

            Application.Current.UserAppTheme = mode switch
            {
                ThemeMode.Light => AppTheme.Light,
                ThemeMode.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
            ApplySystemBars();
        });

    public static void ApplySystemBars()
    {
#if ANDROID
        var window = Platform.CurrentActivity?.Window;

        if (window is null)
        {
            return;
        }

        var controller = AndroidX.Core.View.WindowCompat.GetInsetsController(window, window.DecorView);
        var background = Microsoft.Maui.Platform.ColorExtensions.ToPlatform(Palette.Background);

        if (controller is not null)
        {
            controller.AppearanceLightStatusBars = !Palette.IsDark;
            controller.AppearanceLightNavigationBars = !Palette.IsDark;
        }

#pragma warning disable CA1422
        window.SetStatusBarColor(background);
        window.SetNavigationBarColor(background);
#pragma warning restore CA1422
#endif
    }
}
