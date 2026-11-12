using Microsoft.Data.Sqlite;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Data.Repositories;

public sealed class SettingsRepository : ISettingsRepository
{
    private readonly SqliteConnectionFactory factory;

    public SettingsRepository(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public Task<AppSettings> GetAsync() => Task.Run(() =>
    {
        using var c = factory.Open();

        return Read(c);
    });

    public Task SaveAsync(AppSettings settings) => Task.Run(() =>
    {
        using var c = factory.Open();
        Write(c, null, settings);
    });

    internal static AppSettings Read(SqliteConnection c) =>
        c.Query(
                reader => new AppSettings
                {
                    ReminderEnabled = reader.GetBool("reminder_enabled"),
                    ReminderTime = SqlValues.ToTimeOnly(reader.GetText("reminder_time")),
                    AdaptiveReminder = reader.GetBool("adaptive_reminder"),
                    Theme = reader.GetEnum<ThemeMode>("theme"),
                    UserName = reader.GetNullableString("user_name"),
                    OnboardingDone = reader.GetBool("onboarding_done"),
                    DismissedTagSuggestions = SqlValues.ToStringList(reader.GetText("dismissed_tag_suggestions"))
                },
                "SELECT reminder_enabled, reminder_time, adaptive_reminder, theme, user_name, onboarding_done, " +
                "dismissed_tag_suggestions FROM settings WHERE id = 1")
            .FirstOrDefault() ?? new AppSettings();

    internal static void Write(SqliteConnection c, SqliteTransaction? transaction, AppSettings settings) =>
        c.Execute(
            transaction,
            "INSERT INTO settings (id, reminder_enabled, reminder_time, adaptive_reminder, theme, user_name, " +
            "onboarding_done, dismissed_tag_suggestions) VALUES (1, $enabled, $time, $adaptive, $theme, $name, " +
            "$onboarding, $dismissed) ON CONFLICT(id) DO UPDATE SET reminder_enabled = excluded.reminder_enabled, " +
            "reminder_time = excluded.reminder_time, adaptive_reminder = excluded.adaptive_reminder, " +
            "theme = excluded.theme, user_name = excluded.user_name, onboarding_done = excluded.onboarding_done, " +
            "dismissed_tag_suggestions = excluded.dismissed_tag_suggestions",
            ("$enabled", settings.ReminderEnabled ? 1 : 0),
            ("$time", SqlValues.FromTimeOnly(settings.ReminderTime)),
            ("$adaptive", settings.AdaptiveReminder ? 1 : 0),
            ("$theme", SqlValues.FromEnum(settings.Theme)),
            ("$name", settings.UserName),
            ("$onboarding", settings.OnboardingDone ? 1 : 0),
            ("$dismissed", SqlValues.FromStringList(settings.DismissedTagSuggestions)));
}
