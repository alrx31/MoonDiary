using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Core.Domain;

public class AppSettingsTests
{
    [Fact]
    public void ReminderEnabled_Default_IsTrue()
    {
        var settings = new AppSettings();

        Assert.True(settings.ReminderEnabled);
    }

    [Fact]
    public void ReminderTime_Default_IsEvening()
    {
        var settings = new AppSettings();

        Assert.Equal(new TimeOnly(18, 0), settings.ReminderTime);
    }

    [Fact]
    public void AdaptiveReminder_Default_IsTrue()
    {
        var settings = new AppSettings();

        Assert.True(settings.AdaptiveReminder);
    }

    [Fact]
    public void Theme_Default_IsDark()
    {
        var settings = new AppSettings();

        Assert.Equal(ThemeMode.Dark, settings.Theme);
    }

    [Fact]
    public void UserName_Default_IsNull()
    {
        var settings = new AppSettings();

        Assert.Null(settings.UserName);
    }

    [Fact]
    public void OnboardingDone_Default_IsFalse()
    {
        var settings = new AppSettings();

        Assert.False(settings.OnboardingDone);
    }

    [Fact]
    public void DismissedTagSuggestions_Default_IsEmpty()
    {
        var settings = new AppSettings();

        Assert.Empty(settings.DismissedTagSuggestions);
    }
}
