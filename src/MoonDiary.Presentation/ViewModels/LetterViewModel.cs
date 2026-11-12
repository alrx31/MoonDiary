using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Letters;
using MoonDiary.Core.Time;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class LetterViewModel : ViewModelBase
{
    private readonly LetterService letters;
    private readonly ReminderScheduler scheduler;
    private readonly DayClock clock;
    private Guid letterId;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    private string written = string.Empty;

    [ObservableProperty]
    private string text = string.Empty;

    public LetterViewModel(
        ViewModelContext context,
        LetterService letters,
        ReminderScheduler scheduler,
        DayClock clock)
        : base(context)
    {
        this.letters = letters;
        this.scheduler = scheduler;
        this.clock = clock;
    }

    public override async Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        try
        {
            var letter = await letters.ReadAsync(parameters.GetGuid(NavParams.LetterId) ?? Guid.Empty);
            var created = DayClock.ToLocal(letter.CreatedAtUtc, clock.LocalOffsetMinutes);

            letterId = letter.Id;
            Title = Ru.FullDate(letter.OpenDate);
            Written = string.Format(
                CultureInfo.InvariantCulture,
                Strings.Letter_Written,
                Ru.FullDate(DateOnly.FromDateTime(created.DateTime)));
            Text = letter.Text;
            await letters.MarkNotifiedAsync(letter.Id);
        }
        catch (DomainException exception)
        {
            await Dialogs.AlertAsync(Strings.Letters_ClosedTitle, exception.Message);
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!await Dialogs.ConfirmAsync(
                Strings.Common_Delete,
                Strings.Letters_DeleteConfirm,
                Strings.Common_Delete,
                Strings.Common_Cancel))
        {
            return;
        }

        if (await RunAsync(() => letters.DeleteAsync(letterId)))
        {
            await scheduler.RefreshAsync();
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private Task BackAsync() => Navigation.GoBackAsync();
}
