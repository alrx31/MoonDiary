using Microsoft.Maui.Controls.Shapes;
using MoonDiary.Services;

namespace MoonDiary.Views;

public abstract class AppPage : ContentPage
{
    private const double ScrimOpacity = 0.55;
    private const double SheetOffset = 480;

    private readonly Grid overlay = new() { IsVisible = false };
    private Action? dismiss;
    private int sheetVersion;

    protected AppPage()
    {
        Shell.SetNavBarIsVisible(this, false);
        Shell.SetTabBarIsVisible(this, false);
        SafeAreaEdges = SafeAreaEdges.All;
        ControlTemplate = new ControlTemplate(() =>
        {
            var root = new Grid();

            root.Add(new ContentPresenter());
            root.Add(overlay);

            return root;
        });
    }

    public async Task<object?> ShowSheetAsync(Func<Action<object?>, View> build)
    {
        dismiss?.Invoke();

        var version = ++sheetVersion;
        var completion = new TaskCompletionSource<object?>();
        var scrim = new BoxView { Color = Colors.Black, Opacity = 0, AutomationId = "SheetScrim" };
        var sheet = new Border
        {
            Content = build(result => completion.TrySetResult(result)),
            VerticalOptions = LayoutOptions.End,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Padding = new Thickness(20, 10, 20, 20),
            TranslationY = SheetOffset
        };
        var tap = new TapGestureRecognizer();

        tap.Tapped += (_, _) => completion.TrySetResult(null);
        scrim.GestureRecognizers.Add(tap);
        Palette.Bind(sheet, BackgroundColorProperty, "SurfaceRaised");
        dismiss = () => completion.TrySetResult(null);

        overlay.Clear();
        overlay.Add(scrim);
        overlay.Add(sheet);
        overlay.IsVisible = true;

        await Task.WhenAll(
            scrim.FadeToAsync(ScrimOpacity, 180),
            sheet.TranslateToAsync(0, 0, 240, Easing.CubicOut));

        var result = await completion.Task;

        if (version != sheetVersion)
        {
            return result;
        }

        dismiss = null;
        await Task.WhenAll(
            scrim.FadeToAsync(0, 150),
            sheet.TranslateToAsync(0, Math.Max(sheet.Height, 120), 170, Easing.CubicIn));

        if (version == sheetVersion)
        {
            overlay.Clear();
            overlay.IsVisible = false;
        }

        return result;
    }

    protected sealed override bool OnBackButtonPressed()
    {
        if (dismiss is not null)
        {
            dismiss();

            return true;
        }

        return OnBack();
    }

    protected virtual bool OnBack() => base.OnBackButtonPressed();
}
