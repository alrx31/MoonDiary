using MoonDiary.Presentation.Services;

namespace MoonDiary.Views;

public sealed class LoadingPage : ContentPage
{
    private readonly StartupCoordinator startup;
    private bool started;

    public LoadingPage(StartupCoordinator startup)
    {
        this.startup = startup;
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        Content = new ActivityIndicator
        {
            IsRunning = true,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            AutomationId = "LoadingIndicator"
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (started)
        {
            return;
        }

        started = true;
        await startup.RunAsync();
    }
}
