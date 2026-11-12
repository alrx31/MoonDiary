using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Startup;

public sealed class StartupService
{
    private readonly ISettingsRepository settings;
    private readonly IEntryRepository entries;
    private readonly IReportRepository reports;
    private readonly DayClock clock;

    public StartupService(
        ISettingsRepository settings,
        IEntryRepository entries,
        IReportRepository reports,
        DayClock clock)
    {
        this.settings = settings;
        this.entries = entries;
        this.reports = reports;
        this.clock = clock;
    }

    public async Task<StartupContext> BuildContextAsync() => new(
        (await settings.GetAsync()).OnboardingDone,
        clock.IsInFillWindow,
        await entries.HasEveningAsync(clock.CurrentDayKey),
        await GetUnshownReportIdAsync());

    public async Task<StartupDecision> DecideAsync() => StartupRouter.Decide(await BuildContextAsync());

    public async Task<StartupDecision> DecideAfterOnboardingAsync() =>
        StartupRouter.AfterOnboarding(await BuildContextAsync());

    public async Task<Guid?> GetUnshownReportIdAsync() => (await reports.GetUnshownAsync())?.Id;
}
