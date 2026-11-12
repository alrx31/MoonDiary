using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Entries;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class DraftsViewModel : ViewModelBase
{
    private readonly EntryService entries;

    [ObservableProperty]
    private IReadOnlyList<DraftRowViewModel> items = [];

    [ObservableProperty]
    private bool isEmpty;

    public DraftsViewModel(ViewModelContext context, EntryService entries)
        : base(context)
    {
        this.entries = entries;
    }

    public override async Task RefreshAsync()
    {
        Items = (await entries.GetDraftsAsync()).Select(item => new DraftRowViewModel(item)).ToList();
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    private Task OpenAsync(DraftRowViewModel row) =>
        Navigation.PushAsync(
            Routes.Wizard,
            NavParams.Of((NavParams.DayKey, row.DayKey), (NavParams.FromDraft, true)));

    [RelayCommand]
    private async Task DeleteAsync(DraftRowViewModel row)
    {
        if (!await Dialogs.ConfirmAsync(
                Strings.Common_Delete,
                Strings.Drafts_DeleteConfirm,
                Strings.Common_Delete,
                Strings.Common_Cancel))
        {
            return;
        }

        await RunAsync(() => entries.DeleteDraftAsync(row.DayKey));
        await RefreshAsync();
    }

    [RelayCommand]
    private Task BackAsync() => Navigation.GoBackAsync();
}
