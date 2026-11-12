using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.ViewModels;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public partial class BottomBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty = BindableProperty.Create(
        nameof(ActiveTab), typeof(AppTab), typeof(BottomBar), AppTab.Calendar,
        propertyChanged: (view, _, _) => ((BottomBar)view).UpdateActive());

    public BottomBar()
    {
        InitializeComponent();
        BindingContext = IPlatformApplication.Current?.Services.GetService<BottomBarViewModel>();
        UpdateActive();
    }

    public AppTab ActiveTab
    {
        get => (AppTab)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeChanged -= OnThemeChanged;

        if (Parent is not null)
        {
            Application.Current.RequestedThemeChanged += OnThemeChanged;
            UpdateActive();
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => UpdateActive();

    private void UpdateActive()
    {
        Paint(CalendarTab, AppTab.Calendar);
        Paint(AnalyticsTab, AppTab.Analytics);
        Paint(LettersTab, AppTab.Letters);
        Paint(UserTab, AppTab.User);
    }

    private void Paint(Button button, AppTab tab)
    {
        button.TextColor = tab == ActiveTab ? Palette.Energy : Palette.TextSecondary;
        button.FontFamily = tab == ActiveTab ? "OpenSansSemibold" : "OpenSansRegular";
    }
}
