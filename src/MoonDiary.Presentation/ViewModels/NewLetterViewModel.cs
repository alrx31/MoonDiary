using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Letters;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class NewLetterViewModel : ViewModelBase
{
    public const int MaxDays = 3650;

    private static readonly int[] PresetDays = [1, 7, 30, 150, 365];

    private readonly LetterService letters;
    private readonly ReminderScheduler scheduler;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSend), nameof(Counter), nameof(ShowCounter))]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string text = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DaysText), nameof(OpenText))]
    [NotifyCanExecuteChangedFor(nameof(DecreaseCommand), nameof(IncreaseCommand))]
    private int days = 7;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TimeText), nameof(OpenText))]
    private TimeOnly time = LetterService.DefaultOpenTime;

    public NewLetterViewModel(ViewModelContext context, LetterService letters, ReminderScheduler scheduler)
        : base(context)
    {
        this.letters = letters;
        this.scheduler = scheduler;
        Presets = PresetDays.Select(value => new DayPresetViewModel(value)).ToList();
        MarkPreset();
    }

    public IReadOnlyList<DayPresetViewModel> Presets { get; }

    public static int MaxLength => TextLimits.LetterText;

    public bool CanSend => !string.IsNullOrWhiteSpace(Text);

    public string Counter => (TextLimits.LetterText - Text.Length).ToString(CultureInfo.InvariantCulture);

    public bool ShowCounter => TextLimits.LetterText - Text.Length < TextLimits.CounterThreshold;

    public DateOnly OpenDate => letters.MinOpenDate.AddDays(Days - 1);

    public string DaysText => Ru.Count(Days, "день", "дня", "дней");

    public string TimeText => Ru.Time(Time);

    public string OpenText =>
        string.Format(CultureInfo.InvariantCulture, Strings.NewLetter_WillOpen, Ru.FullDateTime(OpenDate, Time));

    private bool CanDecrease => Days > 1;

    private bool CanIncrease => Days < MaxDays;

    [RelayCommand]
    private void SelectPreset(DayPresetViewModel preset) => Days = preset.Days;

    [RelayCommand(CanExecute = nameof(CanIncrease))]
    private void Increase() => Days = Math.Min(MaxDays, Days + 1);

    [RelayCommand(CanExecute = nameof(CanDecrease))]
    private void Decrease() => Days = Math.Max(1, Days - 1);

    [RelayCommand]
    private async Task PickTimeAsync()
    {
        if (await Dialogs.PickTimeAsync(Strings.NewLetter_Time, Time) is { } picked)
        {
            Time = picked;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (!CanSend)
        {
            return;
        }

        if (await RunAsync(async () => await letters.CreateAsync(Text, OpenDate, Time)))
        {
            await scheduler.RefreshAsync();
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private Task CloseAsync() => Navigation.GoBackAsync();

    partial void OnDaysChanged(int value) => MarkPreset();

    private void MarkPreset()
    {
        foreach (var preset in Presets)
        {
            preset.IsSelected = preset.Days == Days;
        }
    }
}
