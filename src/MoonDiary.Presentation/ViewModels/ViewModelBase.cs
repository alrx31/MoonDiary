using CommunityToolkit.Mvvm.ComponentModel;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Transfer;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    private readonly ViewModelContext context;

    [ObservableProperty]
    private bool isBusy;

    protected ViewModelBase(ViewModelContext context)
    {
        this.context = context;
    }

    protected INavigationService Navigation => context.Navigation;

    protected IDialogService Dialogs => context.Dialogs;

    protected IErrorLog ErrorLog => context.ErrorLog;

    public virtual Task InitializeAsync(IReadOnlyDictionary<string, object> parameters) => Task.CompletedTask;

    public virtual Task RefreshAsync() => Task.CompletedTask;

    protected async Task<bool> RunAsync(Func<Task> action, string failureMessage = Strings.Common_SaveFailed)
    {
        if (IsBusy)
        {
            return false;
        }

        IsBusy = true;

        try
        {
            await action();

            return true;
        }
        catch (Exception exception) when (exception is DomainException or ImportException)
        {
            await Dialogs.AlertAsync(Strings.Common_Error, exception.Message);

            return false;
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(GetType().Name, exception);
            await Dialogs.AlertAsync(Strings.Common_Error, failureMessage);

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
