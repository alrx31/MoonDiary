using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Letters;
using MoonDiary.Core.Reports;
using MoonDiary.Core.Startup;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.Services;

public sealed class StartupCoordinator
{
    public const string OpenWizard = "wizard";
    public const string OpenLetterPrefix = "letter:";

    private readonly IAppBootstrapper bootstrapper;
    private readonly StartupService startup;
    private readonly WeeklyReportService reports;
    private readonly LetterService letters;
    private readonly ISettingsRepository settings;
    private readonly ReminderScheduler scheduler;
    private readonly IThemeService theme;
    private readonly INavigationService navigation;
    private readonly ILaunchRequest launchRequest;
    private readonly IErrorLog errorLog;

    public StartupCoordinator(
        IAppBootstrapper bootstrapper,
        StartupService startup,
        WeeklyReportService reports,
        LetterService letters,
        ISettingsRepository settings,
        ReminderScheduler scheduler,
        IThemeService theme,
        INavigationService navigation,
        ILaunchRequest launchRequest,
        IErrorLog errorLog)
    {
        this.bootstrapper = bootstrapper;
        this.startup = startup;
        this.reports = reports;
        this.letters = letters;
        this.settings = settings;
        this.scheduler = scheduler;
        this.theme = theme;
        this.navigation = navigation;
        this.launchRequest = launchRequest;
        this.errorLog = errorLog;
    }

    public bool IsReady { get; private set; }

    public async Task RunAsync()
    {
        StartupDecision decision;

        try
        {
            await bootstrapper.InitializeAsync();
            theme.Apply((await settings.GetAsync()).Theme);
            await Task.Run(reports.EnsureLastWeekReportAsync);
            await letters.OpenDueAsync();
            decision = await startup.DecideAsync();
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            errorLog.Write(nameof(StartupCoordinator), exception);
            await navigation.GoToRootAsync(
                Routes.DatabaseError,
                NavParams.Of((NavParams.Failure, exception.Message)));

            return;
        }

        IsReady = true;
        await scheduler.RefreshAsync();
        await ApplyAsync(decision);
    }

    public async Task ContinueAfterOnboardingAsync()
    {
        await scheduler.RefreshAsync();
        await ApplyAsync(await startup.DecideAfterOnboardingAsync());
    }

    public async Task HandleOpenRequestAsync()
    {
        if (!IsReady)
        {
            return;
        }

        var target = launchRequest.TakeOpenTarget();

        if (target == OpenWizard)
        {
            await navigation.GoToTabAsync(AppTab.Calendar);
            await navigation.PushAsync(Routes.Wizard);
        }
        else if (target is not null
                 && target.StartsWith(OpenLetterPrefix, StringComparison.Ordinal)
                 && Guid.TryParse(target[OpenLetterPrefix.Length..], out var letterId))
        {
            await letters.OpenDueAsync();
            await navigation.GoToTabAsync(AppTab.Letters);
            await navigation.PushAsync(Routes.Letter, NavParams.Of((NavParams.LetterId, letterId)));
        }
    }

    private async Task ApplyAsync(StartupDecision decision)
    {
        if (decision.Screen == StartScreen.Onboarding)
        {
            await navigation.GoToRootAsync(Routes.Onboarding);

            return;
        }

        await navigation.GoToTabAsync(AppTab.Calendar);

        var target = launchRequest.TakeOpenTarget();

        if (target is not null
            && target.StartsWith(OpenLetterPrefix, StringComparison.Ordinal)
            && Guid.TryParse(target[OpenLetterPrefix.Length..], out var letterId))
        {
            await navigation.GoToTabAsync(AppTab.Letters);
            await navigation.PushAsync(Routes.Letter, NavParams.Of((NavParams.LetterId, letterId)));

            return;
        }

        if (decision.Screen == StartScreen.Wizard || target == OpenWizard)
        {
            await navigation.PushAsync(
                Routes.Wizard,
                NavParams.Of((NavParams.ReportId, decision.ReportToShowAfterWizard ?? decision.ReportToShowNow)));

            return;
        }

        if (decision.ReportToShowNow is { } reportId)
        {
            await navigation.PushAsync(
                Routes.Report,
                NavParams.Of((NavParams.ReportId, reportId), (NavParams.Auto, true)));
        }
    }
}
