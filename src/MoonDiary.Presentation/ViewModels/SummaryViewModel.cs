using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Resources;
using MoonDiary.Core.Time;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class SummaryViewModel : ViewModelBase
{
    public static readonly TimeSpan AutoCloseDelay = TimeSpan.FromSeconds(5);

    private readonly IEntryRepository entries;
    private readonly DayClock clock;
    private readonly TimeProvider timeProvider;
    private Guid? reportId;
    private bool closed;
    private ITimer? timer;

    [ObservableProperty]
    private string thanks = string.Empty;

    [ObservableProperty]
    private int energy;

    [ObservableProperty]
    private int positivity;

    [ObservableProperty]
    private string filled = string.Empty;

    public SummaryViewModel(
        ViewModelContext context,
        IEntryRepository entries,
        DayClock clock,
        TimeProvider timeProvider)
        : base(context)
    {
        this.entries = entries;
        this.clock = clock;
        this.timeProvider = timeProvider;
    }

    public override async Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        reportId = parameters.GetGuid(NavParams.ReportId);
        Thanks = ThanksMessages.Pick(Random.Shared);

        var entry = await entries.GetEveningAsync(parameters.GetDate(NavParams.DayKey) ?? clock.CurrentDayKey);

        if (entry is not null)
        {
            var cards = 1 + new[] { entry.Done, entry.Achievement, entry.Feelings }
                .Count(text => !string.IsNullOrWhiteSpace(text));

            Energy = entry.Energy;
            Positivity = entry.Positivity;
            Filled = string.Format(CultureInfo.InvariantCulture, Strings.Summary_Filled, cards);
        }

        timer = timeProvider.CreateTimer(
            _ => AutoClose?.Invoke(),
            null,
            AutoCloseDelay,
            Timeout.InfiniteTimeSpan);
    }

    public Action? AutoClose { get; set; }

    [RelayCommand]
    public async Task DoneAsync()
    {
        if (closed)
        {
            return;
        }

        closed = true;
        timer?.Dispose();

        if (reportId is { } id)
        {
            await Navigation.ReplaceAsync(
                Routes.Report,
                NavParams.Of((NavParams.ReportId, id), (NavParams.Auto, true)));
        }
        else
        {
            await Navigation.GoBackAsync();
        }
    }
}
