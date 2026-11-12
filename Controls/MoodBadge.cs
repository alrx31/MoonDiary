using MoonDiary.Core.Domain;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class MoodBadge : GraphicsView, IDrawable
{
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(int), typeof(MoodBadge), 0,
        propertyChanged: (view, _, _) => ((MoodBadge)view).Invalidate());

    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(MoodScale), typeof(MoodBadge), MoodScale.Energy,
        propertyChanged: (view, _, _) => ((MoodBadge)view).Invalidate());

    public MoodBadge()
    {
        Drawable = this;
        WidthRequest = 64;
        HeightRequest = 12;
        InputTransparent = true;
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

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float gap = 3;
        var width = (dirtyRect.Width - (gap * 4)) / 5;
        var height = Math.Min(dirtyRect.Height, 8);
        var top = (dirtyRect.Height - height) / 2;

        for (var level = 1; level <= 5; level++)
        {
            var left = (level - 1) * (width + gap);

            if (level <= Value)
            {
                canvas.FillColor = Palette.Level(Kind, Math.Max(Value, 3));
                canvas.FillRoundedRectangle(left, top, width, height, height / 2);
            }
            else
            {
                canvas.StrokeColor = Palette.Border;
                canvas.StrokeSize = 1;
                canvas.DrawRoundedRectangle(left + 0.5f, top + 0.5f, width - 1, height - 1, height / 2);
            }
        }
    }
}
