using MoonDiary.Core.Startup;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Core.Startup;

public class StartupTests
{
    private static readonly Guid ReportId = Guid.NewGuid();

    [Fact]
    public void Decide_InWindowWithoutEntry_OpensWizard()
    {
        var decision = StartupRouter.Decide(new StartupContext(true, true, false, null));

        Assert.Equal(new StartupDecision(StartScreen.Wizard, null, null), decision);
    }

    [Fact]
    public void Decide_InWindowWithEntry_OpensCalendar()
    {
        var decision = StartupRouter.Decide(new StartupContext(true, true, true, null));

        Assert.Equal(new StartupDecision(StartScreen.Calendar, null, null), decision);
    }

    [Fact]
    public void Decide_EntryExistsAndUnshownReport_ShowsReportNow()
    {
        var decision = StartupRouter.Decide(new StartupContext(true, true, true, ReportId));

        Assert.Equal(new StartupDecision(StartScreen.Calendar, ReportId, null), decision);
    }

    [Fact]
    public void Decide_NoEntryAndUnshownReport_ShowsReportAfterWizard()
    {
        var decision = StartupRouter.Decide(new StartupContext(true, true, false, ReportId));

        Assert.Equal(new StartupDecision(StartScreen.Wizard, null, ReportId), decision);
    }

    [Fact]
    public void Decide_OutsideWindow_OpensCalendar()
    {
        var decision = StartupRouter.Decide(new StartupContext(true, false, false, null));

        Assert.Equal(StartScreen.Calendar, decision.Screen);
    }

    [Fact]
    public void Decide_FirstLaunch_OpensOnboarding()
    {
        var context = new StartupContext(false, true, false, ReportId);

        Assert.Equal(StartScreen.Onboarding, StartupRouter.Decide(context).Screen);
        Assert.Equal(StartScreen.Wizard, StartupRouter.AfterOnboarding(context).Screen);
    }

    [Theory]
    [InlineData(10, 18, 0, StartScreen.Wizard)]
    [InlineData(10, 17, 59, StartScreen.Calendar)]
    [InlineData(11, 3, 59, StartScreen.Wizard)]
    [InlineData(11, 4, 0, StartScreen.Calendar)]
    public async Task DecideAsync_WindowBounds_PickScreen(int day, int hour, int minute, StartScreen expected)
    {
        using var app = TestApp.At(2026, 11, day, hour, minute);
        await CompleteOnboardingAsync(app);

        Assert.Equal(expected, (await app.StartupService.DecideAsync()).Screen);
    }

    [Fact]
    public async Task DecideAsync_AtNightWithEntryForPreviousDate_OpensCalendar()
    {
        using var app = TestApp.At(2026, 11, 11, 1, 0);
        await CompleteOnboardingAsync(app);
        await app.Entries.SaveAsync(EntryBuilder.Evening(Dates.D(2026, 11, 10)).Build());

        Assert.Equal(StartScreen.Calendar, (await app.StartupService.DecideAsync()).Screen);
    }

    [Fact]
    public async Task DecideAsync_TimezoneChange_UsesNewLocalTime()
    {
        using var app = new TestApp(TestTime.At(2026, 11, 10, 17, 30, TimeSpan.FromHours(3)));
        await CompleteOnboardingAsync(app);
        Assert.Equal(StartScreen.Calendar, (await app.StartupService.DecideAsync()).Screen);

        app.Time.SetLocalTimeZone(TimeZoneInfo.CreateCustomTimeZone("Other", TimeSpan.FromHours(5), "Other", "Other"));

        Assert.Equal(StartScreen.Wizard, (await app.StartupService.DecideAsync()).Screen);
    }

    [Fact]
    public async Task DecideAsync_FirstLaunch_OpensOnboarding()
    {
        using var app = TestApp.At(2026, 11, 10, 19, 0);

        Assert.Equal(StartScreen.Onboarding, (await app.StartupService.DecideAsync()).Screen);
    }

    private static async Task CompleteOnboardingAsync(TestApp app)
    {
        var settings = await app.Settings.GetAsync();
        settings.OnboardingDone = true;
        await app.Settings.SaveAsync(settings);
    }
}
