using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Time;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class CalendarViewModel : ViewModelBase
{
    private static readonly TimeSpan FeedDelay = TimeSpan.FromMilliseconds(420);

    private readonly CalendarService calendar;
    private readonly DayClock clock;
    private int year;
    private int month;
    private int loadVersion;

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

    public override Task RefreshAsync() => LoadAsync(false);

    [RelayCommand(AllowConcurrentExecutions = true)]
    private Task PreviousAsync()
    {
        var first = new DateOnly(year, month, 1).AddMonths(-1);
        year = first.Year;
        month = first.Month;
        CanGoNext = true;

        return LoadAsync(true);
    }

    [RelayCommand(CanExecute = nameof(CanGoNext), AllowConcurrentExecutions = true)]
    private Task NextAsync()
    {
        if (!CanGoNext)
        {
            return Task.CompletedTask;
        }

        var today = clock.CurrentDayKey;
        var first = new DateOnly(year, month, 1).AddMonths(1);

        if (first > new DateOnly(today.Year, today.Month, 1))
        {
            CanGoNext = false;

            return Task.CompletedTask;
        }

        year = first.Year;
        month = first.Month;

        return LoadAsync(true);
    }

    [RelayCommand]
    private Task OpenDayAsync(DateOnly date) =>
        date > clock.CurrentDayKey
            ? Task.CompletedTask
            : Navigation.PushAsync(Routes.Day, NavParams.Of((NavParams.DayKey, date)));

    private async Task LoadAsync(bool sliding)
    {
        var version = ++loadVersion;

        try
        {
            var data = await calendar.GetMonthAsync(year, month);

            if (version != loadVersion)
            {
                return;
            }

            MonthTitle = Ru.MonthTitle(year, month);
            Days = data.Days;
            CanGoNext = data.CanGoNext;

            if (sliding)
            {
                await Task.Delay(FeedDelay);

                if (version != loadVersion)
                {
                    return;
                }
            }

            Feed = data.Days.Where(day => !day.IsFuture).Reverse().Select(day => new FeedRowViewModel(day)).ToList();
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
