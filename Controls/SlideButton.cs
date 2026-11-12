using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class SlideButton : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(SlideButton), string.Empty,
        propertyChanged: (view, _, value) => ((SlideButton)view).hint.Text = (string)value);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(SlideButton));

    private const double TrackHeight = 52;
    private const double ThumbSize = 44;
    private const double Inset = 4;
    private const double CompleteShare = 0.8;

    private readonly Label hint = new()
    {
        FontSize = 14,
        HorizontalTextAlignment = TextAlignment.Center,
        VerticalOptions = LayoutOptions.Center,
        Margin = new Thickness(ThumbSize + Inset, 0, Inset, 0),
        InputTransparent = true
    };

    private readonly Border thumb = new()
    {
        WidthRequest = ThumbSize,
        HeightRequest = ThumbSize,
        StrokeThickness = 0,
        StrokeShape = new RoundRectangle { CornerRadius = ThumbSize / 2 },
        HorizontalOptions = LayoutOptions.Start,
        VerticalOptions = LayoutOptions.Center,
        Margin = new Thickness(Inset, 0),
        InputTransparent = true
    };

    private double travel;
    private bool completing;

    public SlideButton()
    {
        var arrow = new Label
        {
            Text = "»",
            FontSize = 22,
            FontFamily = "OpenSansSemibold",
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };
        var track = new Border
        {
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = TrackHeight / 2 },
            InputTransparent = true
        };
        var root = new Grid { HeightRequest = TrackHeight };
        var pan = new PanGestureRecognizer();

        Palette.Bind(arrow, Label.TextColorProperty, "OnAccent");
        Palette.Bind(thumb, BackgroundColorProperty, "Energy");
        Palette.Bind(track, BackgroundColorProperty, "Surface");
        Palette.BindBrush(track, Border.StrokeProperty, "Border");
        Palette.Bind(hint, Label.TextColorProperty, "TextSecondary");
        Palette.Bind(root, BackgroundColorProperty, "Background");
        thumb.Content = arrow;
        pan.PanUpdated += OnPan;
        root.GestureRecognizers.Add(pan);
        root.Add(track);
        root.Add(hint);
        root.Add(thumb);
        Content = root;
        SizeChanged += (_, _) => ExcludeSystemGestures();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private double MaxTravel => Math.Max(0, Width - ThumbSize - (2 * Inset));

    private void OnPan(object? sender, PanUpdatedEventArgs args)
    {
        if (completing || MaxTravel <= 0)
        {
            return;
        }

        switch (args.StatusType)
        {
            case GestureStatus.Started:
                thumb.CancelAnimations();
                Move(0);
                break;
            case GestureStatus.Running:
                Move(Math.Clamp(args.TotalX, 0, MaxTravel));
                break;
            default:
                if (travel >= MaxTravel * CompleteShare)
                {
                    _ = CompleteAsync();
                }
                else
                {
                    _ = ReturnAsync();
                }

                break;
        }
    }

    private void Move(double value)
    {
        travel = value;
        thumb.TranslationX = value;
        hint.Opacity = 1 - Math.Min(1, value / (MaxTravel * CompleteShare));
    }

    private async Task ReturnAsync()
    {
        travel = 0;
        await Task.WhenAll(
            thumb.TranslateToAsync(0, 0, 220, Easing.SpringOut),
            hint.FadeToAsync(1, 160));
    }

    private async Task CompleteAsync()
    {
        completing = true;
        await thumb.TranslateToAsync(MaxTravel, 0, 90, Easing.CubicOut);

        if (Command?.CanExecute(null) == true)
        {
            Command.Execute(null);
        }

        await Task.Delay(180);
        Move(0);
        hint.Opacity = 0;
        thumb.Opacity = 0;
        await Task.WhenAll(thumb.FadeToAsync(1, 160), hint.FadeToAsync(1, 160));
        completing = false;
    }

    private void ExcludeSystemGestures()
    {
#if ANDROID
        if (OperatingSystem.IsAndroidVersionAtLeast(29)
            && Handler?.PlatformView is Android.Views.View view
            && view.Width > 0)
        {
            view.SystemGestureExclusionRects = [new Android.Graphics.Rect(0, 0, view.Width, view.Height)];
        }
#endif
    }
}
