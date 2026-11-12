using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Letters;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class LettersViewModel : ViewModelBase
{
    private readonly LetterService letters;
    private readonly ReminderScheduler scheduler;

    [ObservableProperty]
    private IReadOnlyList<LetterRowViewModel> items = [];

    [ObservableProperty]
    private bool isEmpty;

    public LettersViewModel(ViewModelContext context, LetterService letters, ReminderScheduler scheduler)
        : base(context)
    {
        this.letters = letters;
        this.scheduler = scheduler;
    }

    public override async Task RefreshAsync()
    {
        try
        {
            await letters.OpenDueAsync();
            Items = (await letters.GetListAsync()).Select((item, index) => new LetterRowViewModel(item, index)).ToList();
            IsEmpty = Items.Count == 0;
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(LettersViewModel), exception);
        }
    }

    [RelayCommand]
    private Task NewAsync() => Navigation.PushAsync(Routes.NewLetter);

    [RelayCommand]
    private async Task OpenAsync(LetterRowViewModel row)
    {
        if (row.IsOpen)
        {
            await Navigation.PushAsync(Routes.Letter, NavParams.Of((NavParams.LetterId, row.Id)));

            return;
        }

        var choice = await Dialogs.ChooseAsync(
            string.Format(CultureInfo.InvariantCulture, Strings.Letters_ClosedMessage, Ru.FullDateTime(row.OpenDate, row.OpenTime)),
            [],
            Strings.Common_Delete);

        if (choice == Strings.Common_Delete
            && await Dialogs.ConfirmAsync(
                Strings.Common_Delete,
                Strings.Letters_DeleteConfirm,
                Strings.Common_Delete,
                Strings.Common_Cancel))
        {
            await RunAsync(() => letters.DeleteAsync(row.Id));
            await scheduler.RefreshAsync();
            await RefreshAsync();
        }
    }
}
