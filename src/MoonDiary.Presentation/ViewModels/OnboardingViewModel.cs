using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Abstractions;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class OnboardingViewModel : ViewModelBase
{
    private readonly ISettingsRepository settings;
    private readonly INotificationService notifications;
    private readonly StartupCoordinator startup;

    public OnboardingViewModel(
        ViewModelContext context,
        ISettingsRepository settings,
        INotificationService notifications,
        StartupCoordinator startup)
        : base(context)
    {
        this.settings = settings;
        this.notifications = notifications;
        this.startup = startup;
    }

    [RelayCommand]
    private async Task AllowAsync()
    {
        await notifications.RequestPermissionAsync();
        await FinishAsync();
    }

    [RelayCommand]
    private Task LaterAsync() => FinishAsync();

    private async Task FinishAsync()
    {
        var saved = await RunAsync(async () =>
        {
            var current = await settings.GetAsync();
            current.OnboardingDone = true;
            await settings.SaveAsync(current);
        });

        if (saved)
        {
            await startup.ContinueAfterOnboardingAsync();
        }
    }
}
