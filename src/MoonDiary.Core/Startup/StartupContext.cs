namespace MoonDiary.Core.Startup;

public sealed record StartupContext(
    bool OnboardingDone,
    bool InFillWindow,
    bool HasEveningForCurrentDay,
    Guid? UnshownReportId);
