using System.Globalization;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Insights;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class DynamicsChartView : GraphicsView, IDrawable
{
    public static readonly BindableProperty PointsProperty = BindableProperty.Create(
        nameof(Points), typeof(IReadOnlyList<DynamicsPoint>), typeof(DynamicsChartView), null,
        propertyChanged: (view, _, _) => ((DynamicsChartView)view).Invalidate());

    public DynamicsChartView()
    {
        Drawable = this;
        HeightRequest = 160;
        InputTransparent = true;
    }

    public IReadOnlyList<DynamicsPoint>? Points
    {
        get => (IReadOnlyList<DynamicsPoint>?)GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        const float axis = 18;
        const float bottom = 18;
        var plot = new RectF(axis, 6, dirtyRect.Width - axis - 6, dirtyRect.Height - bottom - 6);

        canvas.FontSize = 11;
        canvas.FontColor = Palette.TextMuted;
        canvas.StrokeColor = Palette.Border;
        canvas.StrokeSize = 1;

        for (var level = 1; level <= 5; level++)
        {
            var y = PointY(plot, level);

            canvas.DrawLine(plot.Left, y, plot.Right, y);
            canvas.DrawString(
                level.ToString(CultureInfo.InvariantCulture),
                0, y - 8, axis - 4, 16,
                HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        if (Points is not { Count: > 1 })
        {
            return;
        }

        for (var index = 0; index < Points.Count; index += 7)
        {
            canvas.DrawString(
                Points[index].Date.ToString("dd.MM", CultureInfo.InvariantCulture),
                PointX(plot, index) - 20, plot.Bottom + 2, 40, bottom - 2,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        DrawSeries(canvas, plot, MoodScale.Positivity);
        DrawSeries(canvas, plot, MoodScale.Energy);
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
        }
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

    private void DrawSeries(ICanvas canvas, RectF plot, MoodScale scale)
    {
        var color = Palette.Accent(scale);
        PointF? previous = null;

        canvas.StrokeColor = color;
        canvas.FillColor = color;
        canvas.StrokeSize = 2;
        canvas.StrokeDashPattern = scale == MoodScale.Positivity ? [4, 3] : null;

        for (var index = 0; index < Points!.Count; index++)
        {
            if (Points[index].Mood is not { } mood)
            {
                previous = null;

                continue;
            }

            var point = new PointF(PointX(plot, index), PointY(plot, scale == MoodScale.Energy ? mood.Energy : mood.Positivity));

            if (previous is { } last)
            {
                canvas.DrawLine(last, point);
            }

            if (scale == MoodScale.Energy)
            {
                canvas.FillCircle(point, 3);
            }
            else
            {
                canvas.FillRectangle(point.X - 2.5f, point.Y - 2.5f, 5, 5);
            }

            previous = point;
        }

        canvas.StrokeDashPattern = null;
    }

    private float PointX(RectF plot, int index) => plot.Left + (plot.Width * index / (Points!.Count - 1));

    private static float PointY(RectF plot, int level) => plot.Bottom - (plot.Height * (level - 1) / 4f);
}
