using System.Globalization;
using MoonDiary.Core.Domain;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class MoodScaleView : ContentView
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(int), typeof(MoodScaleView), 0, BindingMode.TwoWay,
        propertyChanged: (view, _, _) => ((MoodScaleView)view).UpdateVisuals());

    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(MoodScale), typeof(MoodScaleView), MoodScale.Energy,
        propertyChanged: (view, _, _) => ((MoodScaleView)view).Build());

    public static readonly BindableProperty IdPrefixProperty = BindableProperty.Create(
        nameof(IdPrefix), typeof(string), typeof(MoodScaleView), string.Empty,
        propertyChanged: (view, _, _) => ((MoodScaleView)view).Build());

    private readonly List<Button> buttons = [];
    private readonly HorizontalStackLayout layout = new() { Spacing = 10, HorizontalOptions = LayoutOptions.Center };

    public MoodScaleView()
    {
        Content = layout;
        Build();
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public MoodScale Kind
    {
        get => (MoodScale)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public string IdPrefix
    {
        get => (string)GetValue(IdPrefixProperty);
        set => SetValue(IdPrefixProperty, value);
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
            UpdateVisuals();
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => UpdateVisuals();

    private void Build()
    {
        layout.Children.Clear();
        buttons.Clear();

        for (var level = MoodValue.Min; level <= MoodValue.Max; level++)
        {
            var value = level;
            var text = value.ToString(CultureInfo.InvariantCulture);
            var button = new Button
            {
                Text = text,
                WidthRequest = 52,
                HeightRequest = 52,
                CornerRadius = 26,
                Padding = 0,
                FontFamily = "OpenSansSemibold",
                FontSize = 16,
                BorderWidth = 2,
                AutomationId = IdPrefix + text
            };

            SemanticProperties.SetDescription(button, $"{ScaleName()} {text}: {MoodLabels.Get(Kind, value)}");
            button.Clicked += (_, _) => Value = value;
            buttons.Add(button);
            layout.Children.Add(button);
        }

        UpdateVisuals();
    }

    private string ScaleName() => Kind == MoodScale.Energy ? "Энергия" : "Позитив";

    private void UpdateVisuals()
    {
        for (var index = 0; index < buttons.Count; index++)
        {
            var level = index + 1;
            var filled = level <= Value;
            var button = buttons[index];

            button.BackgroundColor = filled ? Palette.Level(Kind, level) : Colors.Transparent;
            button.BorderColor = level == Value ? Palette.Accent(Kind) : filled ? Palette.Level(Kind, level) : Palette.Border;
            button.TextColor = filled ? (level >= 3 ? Palette.OnAccent : Palette.TextPrimary) : Palette.TextMuted;
            button.BorderWidth = level == Value ? 3 : 2;
        }
    }
}
