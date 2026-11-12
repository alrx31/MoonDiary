using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Time;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class DayViewModel : ViewModelBase
{
    private readonly CalendarService calendar;
    private readonly EntryService entries;
    private readonly ReminderScheduler scheduler;
    private DateOnly dayKey;
    private Guid? eveningId;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private bool hasEvening;

    [ObservableProperty]
    private bool hasMood;

    [ObservableProperty]
    private bool isFromCheckins;

    [ObservableProperty]
    private int energy;

    [ObservableProperty]
    private int positivity;

    [ObservableProperty]
    private string done = string.Empty;

    [ObservableProperty]
    private string achievement = string.Empty;

    [ObservableProperty]
    private string feelings = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<CheckinRowViewModel> checkins = [];

    [ObservableProperty]
    private bool hasCheckins;

    [ObservableProperty]
    private bool showFill;

    [ObservableProperty]
    private string fillText = Strings.Day_Fill;

    [ObservableProperty]
    private bool showCheckin;

    [ObservableProperty]
    private bool showEmpty;

    public DayViewModel(
        ViewModelContext context,
        CalendarService calendar,
        EntryService entries,
        DayClock clock,
        ReminderScheduler scheduler)
        : base(context)
    {
        this.calendar = calendar;
        this.entries = entries;
        this.scheduler = scheduler;
        dayKey = clock.CurrentDayKey;
    }

    public bool HasDone => Done.Length > 0;

    public bool HasAchievement => Achievement.Length > 0;

    public bool HasFeelings => Feelings.Length > 0;

    public override Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        dayKey = parameters.GetDate(NavParams.DayKey) ?? dayKey;
        Title = Ru.DayTitle(dayKey);

        return Task.CompletedTask;
    }

    public override async Task RefreshAsync()
    {
        DayDetails details;

        try
        {
            details = await calendar.GetDayAsync(dayKey);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(DayViewModel), exception);

            return;
        }

        eveningId = details.Evening?.Id;
        HasEvening = details.Evening is not null;
        HasMood = details.Mood is not null;
        IsFromCheckins = details.Mood is { FromEvening: false };
        Energy = details.Mood?.Energy ?? 0;
        Positivity = details.Mood?.Positivity ?? 0;
        Done = details.Evening?.Done ?? string.Empty;
        Achievement = details.Evening?.Achievement ?? string.Empty;
        Feelings = details.Evening?.Feelings ?? string.Empty;
        Checkins = details.Checkins.Select((entry, index) => new CheckinRowViewModel(entry, index)).ToList();
        HasCheckins = Checkins.Count > 0;
        ShowFill = !HasEvening && !details.IsFuture;
        FillText = details.IsToday ? Strings.Day_Fill : Strings.Day_FillPast;
        ShowCheckin = details.IsToday;
        ShowEmpty = !HasEvening && !HasCheckins;
        OnPropertyChanged(nameof(HasDone));
        OnPropertyChanged(nameof(HasAchievement));
        OnPropertyChanged(nameof(HasFeelings));
    }

    [RelayCommand]
    private Task FillAsync() => Navigation.PushAsync(Routes.Wizard, NavParams.Of((NavParams.DayKey, dayKey)));

    [RelayCommand]
    private Task NewCheckinAsync() => Navigation.PushAsync(Routes.Checkin);

    [RelayCommand]
    private Task OpenCheckinAsync(CheckinRowViewModel row) =>
        Navigation.PushAsync(Routes.Checkin, NavParams.Of((NavParams.EntryId, row.Id)));

    [RelayCommand]
    private Task BackAsync() => Navigation.GoBackAsync();

    [RelayCommand]
    private async Task MenuAsync()
    {
        if (eveningId is not { } id)
        {
            return;
        }

        var choice = await Dialogs.ChooseAsync(
            Strings.Day_Menu,
            [Strings.Common_Edit],
            Strings.Common_Delete);

        if (choice == Strings.Common_Edit)
        {
            await FillAsync();
        }
        else if (choice == Strings.Common_Delete
                 && await Dialogs.ConfirmAsync(
                     Strings.Common_Delete,
                     Strings.Day_DeleteConfirm,
                     Strings.Common_Delete,
                     Strings.Common_Cancel))
        {
            await RunAsync(() => entries.DeleteEveningAsync(id));
            await scheduler.RefreshAsync();
            await RefreshAsync();
        }
    }
}
