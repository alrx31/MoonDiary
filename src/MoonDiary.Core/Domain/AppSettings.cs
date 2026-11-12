namespace MoonDiary.Core.Domain;

public sealed class AppSettings
{
    public bool ReminderEnabled { get; set; } = true;

    public TimeOnly ReminderTime { get; set; } = new(18, 0);

    public bool AdaptiveReminder { get; set; } = true;

    public ThemeMode Theme { get; set; } = ThemeMode.Dark;

    public string? UserName { get; set; }

    public bool OnboardingDone { get; set; }

    public List<string> DismissedTagSuggestions { get; set; } = [];
}
