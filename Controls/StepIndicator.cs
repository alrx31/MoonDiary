using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class StepIndicator : GraphicsView, IDrawable
{
    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step), typeof(int), typeof(StepIndicator), 1,
        propertyChanged: (view, _, _) => ((StepIndicator)view).Invalidate());

    public StepIndicator()
    {
        Drawable = this;
        HeightRequest = 6;
        InputTransparent = true;
    }

    public int Step
    {
        get => (int)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const int count = 4;
        const float gap = 6;
        var width = (dirtyRect.Width - (gap * (count - 1))) / count;

        for (var index = 0; index < count; index++)
        {
            canvas.FillColor = index < Step ? Palette.Energy : Palette.Border;
            canvas.FillRoundedRectangle(index * (width + gap), 0, width, dirtyRect.Height, dirtyRect.Height / 2);
        }
    }
}
