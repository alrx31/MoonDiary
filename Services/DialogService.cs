using MoonDiary.Presentation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class DialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, accept, cancel));

    public Task AlertAsync(string title, string message) =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.DisplayAlertAsync(title, message, Strings.Common_Ok));

    public async Task<string?> ChooseAsync(string title, IReadOnlyList<string> options, string? destructive = null)
    {
        var choice = await MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.DisplayActionSheetAsync(title, Strings.Common_Cancel, destructive, options.ToArray()));

        return choice == Strings.Common_Cancel ? null : choice;
    }
}
