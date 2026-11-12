using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class DatabaseErrorViewModel : ViewModelBase
{
    private readonly IAppBootstrapper bootstrapper;
    private readonly StartupCoordinator startup;

    [ObservableProperty]
    private string details = string.Empty;

    public DatabaseErrorViewModel(ViewModelContext context, IAppBootstrapper bootstrapper, StartupCoordinator startup)
        : base(context)
    {
        this.bootstrapper = bootstrapper;
        this.startup = startup;
    }

    public override Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        Details = parameters.GetString(NavParams.Failure) ?? string.Empty;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task RetryAsync() => startup.RunAsync();

    [RelayCommand]
    private async Task ResetAsync()
    {
        if (!await Dialogs.ConfirmAsync(
                Strings.DatabaseError_Reset,
                Strings.DatabaseError_ResetConfirm,
                Strings.DatabaseError_Reset,
                Strings.Common_Cancel))
        {
            return;
        }

        if (await RunAsync(async () => await bootstrapper.QuarantineDatabaseAsync()))
        {
            await startup.RunAsync();
        }
    }
}
