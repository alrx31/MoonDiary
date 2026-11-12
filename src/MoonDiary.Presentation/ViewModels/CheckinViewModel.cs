using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class CheckinViewModel : ViewModelBase
{
    private readonly CheckinService checkins;
    private Guid? entryId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnergyLabel), nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int energy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositivityLabel), nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private int positivity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Counter), nameof(ShowCounter))]
    private string text = string.Empty;

    [ObservableProperty]
    private bool isEdit;

    public CheckinViewModel(ViewModelContext context, CheckinService checkins)
        : base(context)
    {
        this.checkins = checkins;
    }

    public bool CanSave => Energy > 0 && Positivity > 0;

    public string EnergyLabel => Energy > 0 ? MoodLabels.Get(MoodScale.Energy, Energy) : Strings.Wizard_ChooseScale;

    public string PositivityLabel =>
        Positivity > 0 ? MoodLabels.Get(MoodScale.Positivity, Positivity) : Strings.Wizard_ChooseScale;

    public static int MaxLength => TextLimits.CheckinText;

    public string Counter => (TextLimits.CheckinText - Text.Length).ToString(CultureInfo.InvariantCulture);

    public bool ShowCounter => TextLimits.CheckinText - Text.Length < TextLimits.CounterThreshold;

    public override async Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        entryId = parameters.GetGuid(NavParams.EntryId);

        if (entryId is not { } id)
        {
            return;
        }

        try
        {
            var entry = await checkins.GetAsync(id);

            IsEdit = true;
            Energy = entry.Energy;
            Positivity = entry.Positivity;
            Text = entry.CheckinText ?? string.Empty;
        }
        catch (DomainException exception)
        {
            await Dialogs.AlertAsync(Strings.Common_Error, exception.Message);
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!CanSave)
        {
            return;
        }

        var saved = await RunAsync(async () =>
        {
            if (entryId is { } id)
            {
                await checkins.UpdateAsync(id, Energy, Positivity, Text);
            }
            else
            {
                await checkins.CreateAsync(Energy, Positivity, Text);
            }
        });

        if (saved)
        {
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (entryId is not { } id
            || !await Dialogs.ConfirmAsync(
                Strings.Common_Delete,
                Strings.Checkin_DeleteConfirm,
                Strings.Common_Delete,
                Strings.Common_Cancel))
        {
            return;
        }

        if (await RunAsync(() => checkins.DeleteAsync(id)))
        {
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private Task CloseAsync() => Navigation.GoBackAsync();
}
