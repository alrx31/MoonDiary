namespace MoonDiary.Core.Startup;

public sealed record StartupDecision(StartScreen Screen, Guid? ReportToShowNow, Guid? ReportToShowAfterWizard);
