namespace MoonDiary;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        RequestedThemeChanged += (_, _) => Services.ThemeService.ApplySystemBars();
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new AppShell());
}
