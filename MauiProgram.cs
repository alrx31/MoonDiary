using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Analytics;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Insights;
using MoonDiary.Core.Letters;
using MoonDiary.Core.Reminders;
using MoonDiary.Core.Reports;
using MoonDiary.Core.Startup;
using MoonDiary.Core.Tags;
using MoonDiary.Core.Time;
using MoonDiary.Core.Transfer;
using MoonDiary.Data;
using MoonDiary.Data.Repositories;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;
using MoonDiary.Presentation.ViewModels;
using MoonDiary.Services;
using MoonDiary.Views;

namespace MoonDiary;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if DEBUG && !ANDROID
        LaunchOptions.ReadFromEnvironment();
#endif
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        AddCoreServices(builder.Services);
        AddPlatformServices(builder.Services);
        AddPages(builder.Services);

        return builder.Build();
    }

    private static void AddCoreServices(IServiceCollection services)
    {
        services.AddSingleton<TimeProvider, LaunchTimeProvider>();
        services.AddSingleton<DayClock>();
        services.AddSingleton(_ => new SqliteConnectionFactory(() => LaunchOptions.DatabasePath));
        services.AddSingleton(AnalyticsOptions.Default);

        services.AddSingleton<IEntryRepository, EntryRepository>();
        services.AddSingleton<ITagRepository, TagRepository>();
        services.AddSingleton<IDraftRepository, DraftRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<IReportRepository, ReportRepository>();
        services.AddSingleton<ILetterRepository, LetterRepository>();
        services.AddSingleton<IDataTransferRepository, DataTransferRepository>();

        services.AddSingleton<AnalyticsInputBuilder>();
        services.AddSingleton<CalendarService>();
        services.AddSingleton<EntryService>();
        services.AddSingleton<CheckinService>();
        services.AddSingleton<InsightsService>();
        services.AddSingleton<LetterService>();
        services.AddSingleton<ReminderService>();
        services.AddSingleton<WeeklyReportService>();
        services.AddSingleton<StartupService>();
        services.AddSingleton<TagService>();
        services.AddSingleton<TagSuggestionService>();
        services.AddSingleton(provider => new DataTransferService(
            provider.GetRequiredService<IDataTransferRepository>(),
            provider.GetRequiredService<DayClock>(),
            MigrationRunner.LatestVersion));
    }

    private static void AddPlatformServices(IServiceCollection services)
    {
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IErrorLog, ErrorLog>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IAppBootstrapper, AppBootstrapper>();
        services.AddSingleton<ILaunchRequest, LaunchRequest>();
        services.AddSingleton<DebugSeeder>();
#if ANDROID
        services.AddSingleton<INotificationService, AndroidNotificationService>();
        services.AddSingleton<IBackupSettings, AndroidBackupSettings>();
#else
        services.AddSingleton<INotificationService, DefaultNotificationService>();
        services.AddSingleton<IBackupSettings, DefaultBackupSettings>();
#endif
        services.AddSingleton<ReminderScheduler>();
        services.AddSingleton<StartupCoordinator>();
        services.AddSingleton<ViewModelContext>();
        services.AddSingleton<BottomBarViewModel>();
    }

    private static void AddPages(IServiceCollection services)
    {
        services.AddTransient<LoadingPage>();
        AddPage<OnboardingPage, OnboardingViewModel>(services, null);
        AddPage<CalendarPage, CalendarViewModel>(services, null);
        AddPage<AnalyticsPage, AnalyticsViewModel>(services, null);
        AddPage<LettersPage, LettersViewModel>(services, null);
        AddPage<UserPage, UserViewModel>(services, null);
        AddPage<DatabaseErrorPage, DatabaseErrorViewModel>(services, null);
        AddPage<WizardPage, WizardViewModel>(services, Routes.Wizard);
        AddPage<SummaryPage, SummaryViewModel>(services, Routes.WizardSummary);
        AddPage<CheckinPage, CheckinViewModel>(services, Routes.Checkin);
        AddPage<DayPage, DayViewModel>(services, Routes.Day);
        AddPage<ReportPage, ReportViewModel>(services, Routes.Report);
        AddPage<ReportsPage, ReportsViewModel>(services, Routes.Reports);
        AddPage<DraftsPage, DraftsViewModel>(services, Routes.Drafts);
        AddPage<TagsPage, TagsViewModel>(services, Routes.Tags);
        AddPage<TagFormPage, TagFormViewModel>(services, Routes.TagForm);
        AddPage<SettingsPage, SettingsViewModel>(services, Routes.Settings);
        AddPage<LetterPage, LetterViewModel>(services, Routes.Letter);
        AddPage<NewLetterPage, NewLetterViewModel>(services, Routes.NewLetter);
    }

    private static void AddPage<TPage, TViewModel>(IServiceCollection services, string? route)
        where TPage : Page
        where TViewModel : class
    {
        services.AddTransient<TPage>();
        services.AddTransient<TViewModel>();

        if (route is not null)
        {
            Routing.RegisterRoute(route, typeof(TPage));
        }
    }
}
