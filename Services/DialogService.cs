using MoonDiary.Controls;
using MoonDiary.Presentation;
using MoonDiary.Presentation.Services;
using MoonDiary.Views;

namespace MoonDiary.Services;

public sealed class DialogService : IDialogService
{
    private const int LongTitleLength = 40;

    public async Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        var kind = accept == Strings.Common_Delete ? SheetOptionKind.Destructive : SheetOptionKind.Primary;
        SheetOption[] options =
        [
            new(accept, kind, true),
            new(cancel, SheetOptionKind.Cancel, false)
        ];

        return await ShowAsync(close => SheetViews.Choice(title, message, options, close)) is true;
    }

    public Task AlertAsync(string title, string message)
    {
        SheetOption[] options = [new(Strings.Common_Ok, SheetOptionKind.Primary, true)];

        return ShowAsync(close => SheetViews.Choice(title, message, options, close));
    }

    public async Task<string?> ChooseAsync(string title, IReadOnlyList<string> options, string? destructive = null)
    {
        var list = options.Select(option => new SheetOption(option, SheetOptionKind.Normal, option)).ToList();

        if (destructive is not null)
        {
            list.Add(new SheetOption(destructive, SheetOptionKind.Destructive, destructive));
        }

        list.Add(new SheetOption(Strings.Common_Cancel, SheetOptionKind.Cancel, null));

        var isLong = title.Length > LongTitleLength;

        return await ShowAsync(close => SheetViews.Choice(isLong ? null : title, isLong ? title : null, list, close)) as string;
    }

    public async Task<TimeOnly?> PickTimeAsync(string title, TimeOnly current) =>
        await ShowAsync(close => SheetViews.Time(title, current, close)) as TimeOnly?;

    private static Task<object?> ShowAsync(Func<Action<object?>, View> build) =>
        MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current?.CurrentPage is AppPage page
                ? page.ShowSheetAsync(build)
                : Task.FromResult<object?>(null));
}
