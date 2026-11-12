using MoonDiary.Core.Domain;

namespace MoonDiary.Services;

public static class Palette
{
    public static bool IsDark => Application.Current?.RequestedTheme != AppTheme.Light;

    public static Color Background => Get("Background");

    public static Color Surface => Get("Surface");

    public static Color SurfaceRaised => Get("SurfaceRaised");

    public static Color Border => Get("Border");

    public static Color TextPrimary => Get("TextPrimary");

    public static Color TextSecondary => Get("TextSecondary");

    public static Color TextMuted => Get("TextMuted");

    public static Color Energy => Get("Energy");

    public static Color Positivity => Get("Positivity");

    public static Color OnAccent => Get("OnAccent");

    public static Color Level(MoodScale scale, int value) =>
        Get((scale == MoodScale.Energy ? "Energy" : "Positivity") + Math.Clamp(value, 1, 5));

    public static Color Accent(MoodScale scale) => scale == MoodScale.Energy ? Energy : Positivity;

    private static Color Get(string name)
    {
        var key = (IsDark ? "Dark" : "Light") + name;

        return Application.Current is not null
               && Application.Current.Resources.TryGetValue(key, out var value)
               && value is Color color
            ? color
            : Colors.Gray;
    }
}
