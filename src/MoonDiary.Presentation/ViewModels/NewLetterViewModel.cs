using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Letters;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class NewLetterViewModel : ViewModelBase
{
    private readonly LetterService letters;
    private readonly ReminderScheduler scheduler;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSend), nameof(Counter), nameof(ShowCounter))]
    private string text = string.Empty;

    [ObservableProperty]
    private DateTime openDate;

    public NewLetterViewModel(ViewModelContext context, LetterService letters, ReminderScheduler scheduler)
        : base(context)
    {
        this.letters = letters;
        this.scheduler = scheduler;
        MinDate = letters.MinOpenDate.ToDateTime(TimeOnly.MinValue);
        openDate = MinDate;
    }

    public DateTime MinDate { get; }

    public static int MaxLength => TextLimits.LetterText;

    public bool CanSend => !string.IsNullOrWhiteSpace(Text);

    public string Counter => (TextLimits.LetterText - Text.Length).ToString(CultureInfo.InvariantCulture);

    public bool ShowCounter => TextLimits.LetterText - Text.Length < TextLimits.CounterThreshold;

    [RelayCommand]
    private async Task SendAsync()
    {
        if (!CanSend)
        {
            return;
        }

        if (await RunAsync(async () => await letters.CreateAsync(Text, DateOnly.FromDateTime(OpenDate))))
        {
            await scheduler.RefreshAsync();
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private Task CloseAsync() => Navigation.GoBackAsync();
}
