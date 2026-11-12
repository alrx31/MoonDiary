using MoonDiary.Core.Domain;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;
using MoonDiary.Presentation.Testing;
using MoonDiary.Presentation.ViewModels;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Fakes;

namespace MoonDiary.Tests.Presentation;

public sealed class PresentationTests : IDisposable
{
    private static readonly DateOnly Today = Dates.D(2026, 11, 10);

    private readonly TestApp app = TestApp.At(2026, 11, 10, 19, 30);
    private readonly FakeNavigationService navigation = new();
    private readonly FakePlatform platform = new();

    public void Dispose() => app.Dispose();

    [Fact]
    public void Presentation_DoesNotReferenceMaui()
    {
        var references = typeof(ViewModelBase).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(references, name => name.Name!.StartsWith("Microsoft.Maui", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Wizard_WithoutBothScales_CannotContinueOrSave()
    {
        var wizard = await OpenWizardAsync();

        Assert.False(wizard.ContinueCommand.CanExecute(null));
        Assert.False(wizard.SaveCommand.CanExecute(null));

        wizard.Energy = 4;
        Assert.False(wizard.SaveCommand.CanExecute(null));

        wizard.Positivity = 4;
        Assert.True(wizard.ContinueCommand.CanExecute(null));
        Assert.Equal("бодрость", wizard.EnergyLabel);
    }

    [Fact]
    public async Task Wizard_SaveOnFirstCard_StoresEntryAndOpensSummary()
    {
        var wizard = await OpenWizardAsync();
        wizard.Energy = 4;
        wizard.Positivity = 4;

        await wizard.SaveCommand.ExecuteAsync(null);

        Assert.NotNull(await app.Entries.GetEveningAsync(Today));
        Assert.Equal(("replace", Routes.WizardSummary), (navigation.Last.Method, navigation.Last.Route));
        Assert.Equal(new DateTimeOffset(2026, 11, 11, 18, 0, 0, TestTime.Minsk), platform.Reminder);
    }

    [Fact]
    public async Task Wizard_TagTap_AddsLabelToText()
    {
        await app.SeedTagsAsync();
        var wizard = await OpenWizardAsync();
        wizard.Energy = 3;
        wizard.Positivity = 3;
        await wizard.ContinueCommand.ExecuteAsync(null);

        wizard.ToggleTagCommand.Execute(wizard.Tags[0]);

        Assert.Equal(2, wizard.Step);
        Assert.Equal("работа", wizard.Text);
        Assert.True(wizard.Tags[0].IsSelected);

        wizard.Text = "ничего";
        Assert.False(wizard.Tags[0].IsSelected);
    }

    [Fact]
    public async Task Wizard_Close_SavesDraftAndShowsLinkNextTime()
    {
        var wizard = await OpenWizardAsync();
        wizard.Energy = 2;

        await wizard.CloseCommand.ExecuteAsync(null);

        Assert.Equal("back", navigation.Last.Method);
        Assert.True(await app.EntryService.HasDraftAsync(Today));

        var next = await OpenWizardAsync();
        Assert.True(next.HasDraftLink);
        Assert.Equal(0, next.Energy);

        await next.OpenDraftCommand.ExecuteAsync(null);
        Assert.Equal(2, next.Energy);
    }

    [Fact]
    public async Task Wizard_CloseUntouched_DoesNotCreateDraft()
    {
        var wizard = await OpenWizardAsync();

        await wizard.CloseCommand.ExecuteAsync(null);

        Assert.False(await app.EntryService.HasDraftAsync(Today));
    }

    [Fact]
    public async Task Wizard_SkipFromLastCard_Saves()
    {
        var wizard = await OpenWizardAsync();
        wizard.Energy = 3;
        wizard.Positivity = 3;
        await wizard.SkipCommand.ExecuteAsync(null);
        Assert.Equal(1, wizard.Step);

        await wizard.ContinueCommand.ExecuteAsync(null);
        await wizard.SkipCommand.ExecuteAsync(null);
        await wizard.SkipCommand.ExecuteAsync(null);
        Assert.Equal(4, wizard.Step);

        await wizard.BackCommand.ExecuteAsync(null);
        Assert.Equal(3, wizard.Step);
    }

    [Fact]
    public async Task Summary_WithReport_OpensReportAfterDone()
    {
        await app.EveningAsync(Today, 4, 3);
        var reportId = Guid.NewGuid();
        var summary = new SummaryViewModel(Context(), app.Entries, app.Clock, app.Time);

        await summary.InitializeAsync(NavParams.Of((NavParams.DayKey, Today), (NavParams.ReportId, reportId)));
        await summary.DoneCommand.ExecuteAsync(null);
        await summary.DoneCommand.ExecuteAsync(null);

        Assert.Equal(4, summary.Energy);
        Assert.Contains("1 из 4", summary.Filled);
        Assert.Single(navigation.Calls);
        Assert.Equal(("replace", Routes.Report), (navigation.Last.Method, navigation.Last.Route));
    }

    [Fact]
    public async Task Summary_AfterFiveSeconds_RequestsAutoClose()
    {
        await app.EveningAsync(Today, 4, 3);
        var summary = new SummaryViewModel(Context(), app.Entries, app.Clock, app.Time);
        var closed = false;
        summary.AutoClose = () => closed = true;
        await summary.InitializeAsync(NavParams.Of((NavParams.DayKey, Today)));

        app.Time.Advance(TimeSpan.FromSeconds(4));
        Assert.False(closed);
        app.Time.Advance(TimeSpan.FromSeconds(1));

        Assert.True(closed);
    }

    [Fact]
    public async Task Startup_InWindowWithUnshownReport_OpensWizardWithReportAfter()
    {
        await CompleteOnboardingAsync();
        await app.EveningAsync(Today, 3, 3);
        app.SetLocal(2026, 11, 16, 19, 0);

        await Coordinator().RunAsync();

        Assert.Equal(["tab", "push"], navigation.Calls.Select(call => call.Method));
        Assert.Equal(Routes.Wizard, navigation.Last.Route);
        Assert.NotNull(navigation.Last.Params!.GetGuid(NavParams.ReportId));
        Assert.Equal(ThemeMode.Dark, platform.Theme);
    }

    [Fact]
    public async Task Startup_OutsideWindow_OpensCalendarThenReport()
    {
        await CompleteOnboardingAsync();
        await app.EveningAsync(Today, 3, 3);
        app.SetLocal(2026, 11, 16, 10, 0);

        await Coordinator().RunAsync();

        Assert.Equal(Routes.Report, navigation.Last.Route);
        Assert.True(navigation.Last.Params!.GetBool(NavParams.Auto));
    }

    [Fact]
    public async Task Startup_FirstLaunchAndDatabaseFailure_OpenTheirScreens()
    {
        await Coordinator().RunAsync();
        Assert.Equal(("root", Routes.Onboarding), (navigation.Last.Method, navigation.Last.Route));

        platform.FailInitialize = true;
        await Coordinator().RunAsync();

        Assert.Equal(Routes.DatabaseError, navigation.Last.Route);
        Assert.Single(platform.Errors);
    }

    [Fact]
    public async Task Checkin_Save_CreatesEntryAndGoesBack()
    {
        var checkin = new CheckinViewModel(Context(), app.CheckinService);
        await checkin.InitializeAsync(NavParams.Of());
        Assert.False(checkin.SaveCommand.CanExecute(null));

        checkin.Energy = 2;
        checkin.Positivity = 3;
        checkin.Text = "стресс";
        await checkin.SaveCommand.ExecuteAsync(null);

        Assert.Equal("стресс", Assert.Single(await app.Entries.GetCheckinsAsync(Today)).CheckinText);
        Assert.Equal("back", navigation.Last.Method);
    }

    [Fact]
    public void LaunchOptionsParser_IgnoresInvalidValues()
    {
        var valid = LaunchOptionsParser.Parse(key => key switch
        {
            LaunchOptionsParser.NowKey => "2026-11-10T18:00:00+03:00",
            LaunchOptionsParser.DatabaseKey => "test.db",
            _ => null
        });
        var invalid = LaunchOptionsParser.Parse(key => key switch
        {
            LaunchOptionsParser.NowKey => "вчера",
            LaunchOptionsParser.DatabaseKey => "../real.db",
            _ => null
        });

        Assert.Equal(new DateTimeOffset(2026, 11, 10, 18, 0, 0, TestTime.Minsk), valid.Now);
        Assert.Equal("test.db", valid.DatabaseFileName);
        Assert.Null(invalid.Now);
        Assert.Null(invalid.DatabaseFileName);
    }

    [Fact]
    public async Task NewLetter_PresetStepperAndTime_DefineOpenMoment()
    {
        var letter = new NewLetterViewModel(Context(), app.LetterService, Scheduler()) { Text = "привет" };
        var start = app.LetterService.MinOpenDate;

        Assert.Equal(7, letter.Days);
        Assert.True(letter.Presets.Single(preset => preset.Days == 7).IsSelected);

        letter.SelectPresetCommand.Execute(letter.Presets.Single(preset => preset.Days == 30));
        letter.IncreaseCommand.Execute(null);
        letter.IncreaseCommand.Execute(null);
        letter.DecreaseCommand.Execute(null);

        Assert.Equal(31, letter.Days);
        Assert.Equal("31 день", letter.DaysText);
        Assert.DoesNotContain(letter.Presets, preset => preset.IsSelected);

        platform.PickedTime = new TimeOnly(21, 45);
        await letter.PickTimeCommand.ExecuteAsync(null);
        await letter.SendCommand.ExecuteAsync(null);

        var saved = Assert.Single(await app.LetterService.GetListAsync());
        Assert.Equal(start.AddDays(30), saved.OpenDate);
        Assert.Equal(new TimeOnly(21, 45), saved.OpenTime);
    }

    [Fact]
    public void NewLetter_OneDay_CannotGoLower()
    {
        var letter = new NewLetterViewModel(Context(), app.LetterService, Scheduler());

        letter.SelectPresetCommand.Execute(letter.Presets[0]);

        Assert.Equal(1, letter.Days);
        Assert.False(letter.DecreaseCommand.CanExecute(null));
        Assert.Equal(app.LetterService.MinOpenDate, letter.OpenDate);
    }

    private ViewModelContext Context() => new(navigation, platform, platform);

    private ReminderScheduler Scheduler() =>
        new(app.ReminderService, app.LetterService, platform, app.Clock, platform);

    private StartupCoordinator Coordinator() => new(
        platform, app.StartupService, app.ReportService, app.LetterService, app.Settings,
        Scheduler(), platform, navigation, platform, platform);

    private async Task<WizardViewModel> OpenWizardAsync()
    {
        var wizard = new WizardViewModel(Context(), app.EntryService, app.TagService, app.Clock, Scheduler());
        await wizard.InitializeAsync(NavParams.Of());

        return wizard;
    }

    private async Task CompleteOnboardingAsync()
    {
        var settings = await app.Settings.GetAsync();
        settings.OnboardingDone = true;
        await app.Settings.SaveAsync(settings);
    }
}
