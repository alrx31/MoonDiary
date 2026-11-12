using System.Globalization;
using MoonDiary.Presentation;
using MoonDiary.Services;

namespace MoonDiary.Controls;

public static class SheetViews
{
    private const int MinuteStep = 5;

    public static View Choice(string? title, string? message, IReadOnlyList<SheetOption> options, Action<object?> close)
    {
        var layout = CreateLayout(title);

        if (!string.IsNullOrEmpty(message))
        {
            layout.Add(new Label { Text = message, AutomationId = "SheetMessage", Margin = new Thickness(0, 0, 0, 8) });
        }

        for (var index = 0; index < options.Count; index++)
        {
            var option = options[index];
            var button = CreateButton(option.Text, option.Kind);

            button.AutomationId = option.Kind == SheetOptionKind.Cancel
                ? "SheetCancel"
                : "SheetOption" + index.ToString(CultureInfo.InvariantCulture);
            button.Clicked += (_, _) => close(option.Result);
            layout.Add(button);
        }

        return layout;
    }

    public static View Time(string title, TimeOnly current, Action<object?> close)
    {
        var hour = current.Hour;
        var minute = current.Minute;
        var layout = CreateLayout(title);
        var hourLabel = CreateDigits("TimeSheetHour");
        var minuteLabel = CreateDigits("TimeSheetMinute");
        var picker = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            ],
            ColumnSpacing = 12,
            Margin = new Thickness(0, 4, 0, 12)
        };
        var done = CreateButton(Strings.Common_Done, SheetOptionKind.Primary);
        var cancel = CreateButton(Strings.Common_Cancel, SheetOptionKind.Cancel);

        void Refresh()
        {
            hourLabel.Text = hour.ToString("00", CultureInfo.InvariantCulture);
            minuteLabel.Text = minute.ToString("00", CultureInfo.InvariantCulture);
        }

        void ShiftHour(int delta)
        {
            hour = (hour + delta + 24) % 24;
            Refresh();
        }

        void ShiftMinute(int delta)
        {
            var aligned = delta > 0 ? minute - (minute % MinuteStep) : minute + ((MinuteStep - (minute % MinuteStep)) % MinuteStep);

            minute = (aligned + (delta * MinuteStep) + 60) % 60;
            Refresh();
        }

        picker.Add(CreateColumn(hourLabel, "TimeSheetHour", ShiftHour), 1);
        picker.Add(
            new Label
            {
                Text = ":",
                Style = Palette.Style("DisplayLabel"),
                FontSize = 40,
                VerticalOptions = LayoutOptions.Center
            },
            2);
        picker.Add(CreateColumn(minuteLabel, "TimeSheetMinute", ShiftMinute), 3);
        done.AutomationId = "SheetDone";
        done.Clicked += (_, _) => close(new TimeOnly(hour, minute));
        cancel.AutomationId = "SheetCancel";
        cancel.Clicked += (_, _) => close(null);
        layout.Add(picker);
        layout.Add(done);
        layout.Add(cancel);
        Refresh();

        return layout;
    }

    private static VerticalStackLayout CreateLayout(string? title)
    {
        var grabber = new BoxView
        {
            WidthRequest = 40,
            HeightRequest = 4,
            CornerRadius = 2,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        var layout = new VerticalStackLayout { Spacing = 8 };

        Palette.Bind(grabber, BoxView.ColorProperty, "Border");
        layout.Add(grabber);

        if (!string.IsNullOrEmpty(title))
        {
            layout.Add(new Label
            {
                Text = title,
                Style = Palette.Style("TitleLabel"),
                AutomationId = "SheetTitle",
                Margin = new Thickness(0, 0, 0, 4)
            });
        }

        return layout;
    }

    private static Button CreateButton(string text, SheetOptionKind kind)
    {
        var button = new Button
        {
            Text = text,
            Style = Palette.Style(kind switch
            {
                SheetOptionKind.Primary => "PrimaryButton",
                SheetOptionKind.Cancel => "TextButton",
                _ => "SecondaryButton"
            })
        };

        if (kind == SheetOptionKind.Destructive)
        {
            Palette.Bind(button, Button.TextColorProperty, "Destructive");
        }
        else if (kind == SheetOptionKind.Cancel)
        {
            Palette.Bind(button, Button.TextColorProperty, "TextSecondary");
        }

        return button;
    }

    private static Label CreateDigits(string automationId) => new()
    {
        Style = Palette.Style("DisplayLabel"),
        FontSize = 40,
        WidthRequest = 72,
        HorizontalTextAlignment = TextAlignment.Center,
        AutomationId = automationId
    };

    private static VerticalStackLayout CreateColumn(Label digits, string idPrefix, Action<int> shift)
    {
        var plus = new Button { Text = "+", Style = Palette.Style("StepButton"), AutomationId = idPrefix + "Plus" };
        var minus = new Button { Text = "−", Style = Palette.Style("StepButton"), AutomationId = idPrefix + "Minus" };

        plus.Clicked += (_, _) => shift(1);
        minus.Clicked += (_, _) => shift(-1);

        return new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children = { plus, digits, minus }
        };
    }
}
