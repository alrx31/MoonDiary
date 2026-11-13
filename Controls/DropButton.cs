using System.Windows.Input;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public sealed class DropButton : GraphicsView, IDrawable
{
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(DropButton));

    public const double Diameter = 56;

    private const double MaxLift = 44;
    private const double CompleteShare = 0.7;
    private const double TapSlop = 12;
    private const float GlyphArm = 9;
    private const float GlyphThickness = 2.5f;
    private const uint SettleRate = 16;
    private const uint SettleLength = 220;

    private double lift;
    private double startY;
    private bool pressed;

    public DropButton()
    {
        Drawable = this;
        WidthRequest = Diameter;
        HeightRequest = Diameter;
        StartInteraction += OnStart;
        DragInteraction += OnDrag;
        EndInteraction += OnEnd;
        CancelInteraction += OnCancel;
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var radius = (float)(Diameter / 2) - 0.5f;
        var center = new PointF(dirtyRect.Center.X, (float)(dirtyRect.Bottom - radius - 0.5 - lift));

        canvas.FillColor = Palette.Energy;

        if (lift > 0.5)
        {
            FillNeck(canvas, center, radius, dirtyRect.Bottom - 0.5f);
        }

        canvas.FillCircle(center, radius);
        DrawGlyph(canvas, center);
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
            Invalidate();
        }
    }

    private static void FillNeck(ICanvas canvas, PointF center, float radius, float tipY)
    {
        var height = tipY - center.Y;
        var beta = Math.Acos(Math.Clamp(radius / height, -1, 1));
        var sideX = radius * (float)Math.Sin(beta);
        var sideY = radius * (float)Math.Cos(beta);
        var shoulder = new PointF(center.X, center.Y + (radius * radius / height));
        var left = new PointF(center.X - sideX, center.Y + sideY);
        var right = new PointF(center.X + sideX, center.Y + sideY);
        var tip = new PointF(center.X, tipY);
        var path = new PathF();

        path.MoveTo(left);
        path.CurveTo(Middle(left, shoulder), Middle(tip, shoulder), tip);
        path.CurveTo(Middle(tip, shoulder), Middle(right, shoulder), right);
        path.Close();
        canvas.FillPath(path);
    }

    private static void DrawGlyph(ICanvas canvas, PointF center)
    {
        canvas.StrokeColor = Palette.OnAccent;
        canvas.StrokeSize = GlyphThickness;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawLine(center.X - GlyphArm, center.Y, center.X + GlyphArm, center.Y);
        canvas.DrawLine(center.X, center.Y - GlyphArm, center.X, center.Y + GlyphArm);
    }

    private static PointF Middle(PointF from, PointF to) => new((from.X + to.X) / 2, (from.Y + to.Y) / 2);

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Invalidate();

    private void OnStart(object? sender, TouchEventArgs args)
    {
        var point = args.Touches.FirstOrDefault();

        if (!Inside(point))
        {
            return;
        }

        this.CancelAnimations();
        pressed = true;
        startY = point.Y;
    }

    private void OnDrag(object? sender, TouchEventArgs args)
    {
        if (!pressed)
        {
            return;
        }

        var value = Math.Clamp(startY - args.Touches.FirstOrDefault().Y, 0, MaxLift);

        if (value > 0 && HeightRequest < Diameter + MaxLift)
        {
            HeightRequest = Diameter + MaxLift;
        }

        SetLift(value);
    }

    private void OnEnd(object? sender, TouchEventArgs args)
    {
        if (!pressed)
        {
            return;
        }

        pressed = false;

        if (lift >= MaxLift * CompleteShare || lift < TapSlop)
        {
            Execute();
            Settle();
            return;
        }

        Settle();
    }

    private void OnCancel(object? sender, EventArgs args)
    {
        if (!pressed)
        {
            return;
        }

        pressed = false;
        Settle();
    }

    private void Settle()
    {
        var from = lift;

        if (from <= 0)
        {
            Shrink();
            return;
        }

        new Animation(SetLift, from, 0)
            .Commit(this, "drop", SettleRate, SettleLength, Easing.CubicInOut, (_, _) => Shrink());
    }

    private void Shrink()
    {
        if (!pressed && lift <= 0)
        {
            HeightRequest = Diameter;
        }
    }

    private void Execute()
    {
        if (Command?.CanExecute(null) == true)
        {
            Command.Execute(null);
        }
    }

    private void SetLift(double value)
    {
        lift = Math.Clamp(value, 0, MaxLift);
        Invalidate();
    }

    private bool Inside(PointF point)
    {
        var radius = Diameter / 2;
        var bottom = Height > 0 ? Height : Diameter;
        var offsetX = point.X - radius;
        var offsetY = point.Y - (bottom - radius - 0.5 - lift);

        return (offsetX * offsetX) + (offsetY * offsetY) <= radius * radius;
    }
}
