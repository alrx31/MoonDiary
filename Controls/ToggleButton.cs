using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class ToggleButton : Button
{
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(ToggleButton), false,
        propertyChanged: (view, _, _) => ((ToggleButton)view).Paint());

    public ToggleButton()
    {
        CornerRadius = 22;
        Padding = new Thickness(14, 8);
        FontFamily = "OpenSansRegular";
        FontSize = 15;
        MinimumHeightRequest = 44;
        LineBreakMode = LineBreakMode.NoWrap;
        Paint();
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
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
            Paint();
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Paint();

    private void Paint()
    {
        BackgroundColor = IsSelected ? Palette.Energy : Palette.Surface;
        TextColor = IsSelected ? Palette.OnAccent : Palette.TextPrimary;
        BorderColor = IsSelected ? Palette.Energy : Palette.Border;
        BorderWidth = IsSelected ? 2 : 1;
        FontFamily = IsSelected ? "OpenSansSemibold" : "OpenSansRegular";
    }
}
