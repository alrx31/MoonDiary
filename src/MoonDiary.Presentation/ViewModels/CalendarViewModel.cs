using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Time;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class CalendarViewModel : ViewModelBase
{
    private readonly CalendarService calendar;
    private readonly DayClock clock;
    private int year;
    private int month;

    [ObservableProperty]
    private string monthTitle = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<CalendarDay> days = [];

    [ObservableProperty]
    private IReadOnlyList<FeedRowViewModel> feed = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private bool canGoNext;

    [ObservableProperty]
    private bool isEmpty;

    public CalendarViewModel(ViewModelContext context, CalendarService calendar, DayClock clock)
        : base(context)
    {
        this.calendar = calendar;
        this.clock = clock;

        var today = clock.CurrentDayKey;
        year = today.Year;
        month = today.Month;
    }

    public override Task RefreshAsync() => LoadAsync();

    [RelayCommand]
    private Task PreviousAsync()
    {
        var first = new DateOnly(year, month, 1).AddMonths(-1);
        year = first.Year;
        month = first.Month;

        return LoadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private Task NextAsync()
    {
        if (!CanGoNext)
        {
            return Task.CompletedTask;
        }

        var first = new DateOnly(year, month, 1).AddMonths(1);
        year = first.Year;
        month = first.Month;

        return LoadAsync();
    }

    [RelayCommand]
    private Task OpenDayAsync(DateOnly date) =>
        date > clock.CurrentDayKey
            ? Task.CompletedTask
            : Navigation.PushAsync(Routes.Day, NavParams.Of((NavParams.DayKey, date)));

    [RelayCommand]
    private Task CheckinAsync() => Navigation.PushAsync(Routes.Checkin);

    private async Task LoadAsync()
    {
        try
        {
            var data = await calendar.GetMonthAsync(year, month);

            MonthTitle = Ru.MonthTitle(year, month);
            Days = data.Days;
            Feed = data.Days.Where(day => !day.IsFuture).Reverse().Select(day => new FeedRowViewModel(day)).ToList();
            CanGoNext = data.CanGoNext;
            IsEmpty = data.Days.All(day => day.Mood is null);
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(CalendarViewModel), exception);
        }
    }
}
