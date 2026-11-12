namespace MoonDiary.Core.Startup;

public static class StartupRouter
{
    public static StartupDecision Decide(StartupContext context) =>
        context.OnboardingDone
            ? AfterOnboarding(context)
            : new StartupDecision(StartScreen.Onboarding, null, null);

    public static StartupDecision AfterOnboarding(StartupContext context) =>
        context.InFillWindow && !context.HasEveningForCurrentDay
            ? new StartupDecision(StartScreen.Wizard, null, context.UnshownReportId)
            : new StartupDecision(StartScreen.Calendar, context.UnshownReportId, null);
}
