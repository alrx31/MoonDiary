using System.Globalization;
using System.Windows.Input;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Domain;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class CalendarView : GraphicsView, IDrawable
{
    public static readonly BindableProperty DaysProperty = BindableProperty.Create(
        nameof(Days), typeof(IReadOnlyList<CalendarDay>), typeof(CalendarView), null,
        propertyChanged: (view, _, _) => ((CalendarView)view).OnDaysChanged());

    public static readonly BindableProperty DayTappedCommandProperty = BindableProperty.Create(
        nameof(DayTappedCommand), typeof(ICommand), typeof(CalendarView));

    public static readonly BindableProperty PreviousCommandProperty = BindableProperty.Create(
        nameof(PreviousCommand), typeof(ICommand), typeof(CalendarView));

    public static readonly BindableProperty NextCommandProperty = BindableProperty.Create(
        nameof(NextCommand), typeof(ICommand), typeof(CalendarView));

    private const float HeaderHeight = 28;
    private const float CellHeight = 58;
    private const float SwipeDistance = 60;
    private const double SlideShare = 0.22;

    private PointF? start;
    private IReadOnlyList<CalendarDay>? shown;
    private int offset;
    private int rows = 5;
    private int slideVersion;

    public CalendarView()
    {
        Drawable = this;
        HeightRequest = HeaderHeight + (CellHeight * rows);
        StartInteraction += (_, args) => start = args.Touches.FirstOrDefault();
        EndInteraction += OnEndInteraction;
        CancelInteraction += (_, _) => start = null;
    }

    public IReadOnlyList<CalendarDay>? Days
    {
        get => (IReadOnlyList<CalendarDay>?)GetValue(DaysProperty);
        set => SetValue(DaysProperty, value);
    }

    public ICommand? DayTappedCommand
    {
        get => (ICommand?)GetValue(DayTappedCommandProperty);
        set => SetValue(DayTappedCommandProperty, value);
    }

    public ICommand? PreviousCommand
    {
        get => (ICommand?)GetValue(PreviousCommandProperty);
        set => SetValue(PreviousCommandProperty, value);
    }

    public ICommand? NextCommand
    {
        get => (ICommand?)GetValue(NextCommandProperty);
        set => SetValue(NextCommandProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var cellWidth = dirtyRect.Width / 7;

        canvas.FontSize = 12;
        canvas.FontColor = Palette.TextMuted;

        for (var column = 0; column < 7; column++)
        {
            canvas.DrawString(
                Ru.WeekdayHeaders[column],
                column * cellWidth, 0, cellWidth, HeaderHeight,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }

        if (shown is null)
        {
            return;
        }

        for (var index = 0; index < shown.Count; index++)
        {
            var day = shown[index];
            var cell = index + offset;
            var left = (cell % 7) * cellWidth;
            var top = HeaderHeight + ((cell / 7) * CellHeight);

            DrawDay(canvas, day, new RectF(left + 3, top + 3, cellWidth - 6, CellHeight - 6));
        }
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

    private static void DrawDay(ICanvas canvas, CalendarDay day, RectF rect)
    {
        if (day.Mood is not null)
        {
            canvas.FillColor = Palette.Surface;
            canvas.FillRoundedRectangle(rect, 10);
        }

        if (day.IsToday)
        {
            canvas.StrokeColor = Palette.Energy;
            canvas.StrokeSize = 2;
            canvas.DrawRoundedRectangle(rect, 10);
        }

        canvas.FontSize = 14;
        canvas.FontColor = day.IsFuture
            ? Palette.Border
            : day.Mood is null ? Palette.TextMuted : Palette.TextPrimary;
        canvas.DrawString(
            day.Date.Day.ToString(CultureInfo.InvariantCulture),
            rect.Left, rect.Top + 4, rect.Width, 20,
            HorizontalAlignment.Center, VerticalAlignment.Center);

        if (day.Mood is null)
        {
            return;
        }

        DrawIndicator(canvas, MoodScale.Energy, day.Mood.Energy, rect, rect.Top + 30);
        DrawIndicator(canvas, MoodScale.Positivity, day.Mood.Positivity, rect, rect.Top + 40);

        if (!day.HasEvening)
        {
            canvas.FillColor = Palette.TextMuted;
            canvas.FillCircle(rect.Right - 7, rect.Top + 7, 2);
        }
    }

    private static void DrawIndicator(ICanvas canvas, MoodScale scale, int value, RectF rect, float top)
    {
        const float height = 5;
        var maxWidth = rect.Width - 12;
        var left = rect.Left + 6;

        canvas.FillColor = Palette.Border;
        canvas.FillRoundedRectangle(left, top, maxWidth, height, height / 2);
        canvas.FillColor = Palette.Level(scale, Math.Max(value, 2));
        canvas.FillRoundedRectangle(left, top, maxWidth * value / 5f, height, height / 2);
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

    private void OnDaysChanged()
    {
        var next = Days;

        if (shown is { Count: > 0 } && next is { Count: > 0 } && Width > 0 && !SameMonth(shown[0].Date, next[0].Date))
        {
            _ = SlideAsync(next, next[0].Date > shown[0].Date ? -1 : 1);
        }
        else
        {
            Apply(next);
        }
    }

    private static bool SameMonth(DateOnly left, DateOnly right) => left.Year == right.Year && left.Month == right.Month;

    private void Apply(IReadOnlyList<CalendarDay>? days)
    {
        shown = days;

        if (days is { Count: > 0 })
        {
            offset = ((int)days[0].Date.DayOfWeek + 6) % 7;
            rows = (int)Math.Ceiling((offset + days.Count) / 7.0);
            HeightRequest = HeaderHeight + (CellHeight * rows);
        }

        Invalidate();
    }

    private async Task SlideAsync(IReadOnlyList<CalendarDay> days, int direction)
    {
        var version = ++slideVersion;
        var shift = Width * SlideShare;

        this.CancelAnimations();
        await Task.WhenAll(
            this.TranslateToAsync(direction * shift, 0, 120, Easing.CubicIn),
            this.FadeToAsync(0, 120, Easing.CubicIn));

        if (version != slideVersion)
        {
            return;
        }

        Apply(days);
        TranslationX = -direction * shift;
        await Task.WhenAll(
            this.TranslateToAsync(0, 0, 220, Easing.CubicOut),
            this.FadeToAsync(1, 220, Easing.CubicOut));
    }

    private void OnEndInteraction(object? sender, TouchEventArgs args)
    {
        var end = args.Touches.FirstOrDefault();

        if (start is not { } from)
        {
            return;
        }

        start = null;

        var dx = end.X - from.X;

        if (Math.Abs(dx) >= SwipeDistance && Math.Abs(dx) > Math.Abs(end.Y - from.Y))
        {
            var command = dx > 0 ? PreviousCommand : NextCommand;

            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
            }

            return;
        }

        if (shown is null || Width <= 0 || end.Y < HeaderHeight)
        {
            return;
        }

        var column = (int)(end.X / (Width / 7));
        var row = (int)((end.Y - HeaderHeight) / CellHeight);
        var index = (row * 7) + column - offset;

        if (index >= 0 && index < shown.Count && DayTappedCommand?.CanExecute(shown[index].Date) == true)
        {
            DayTappedCommand.Execute(shown[index].Date);
        }
    }
}
