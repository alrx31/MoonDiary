using Microsoft.Maui.Controls.Shapes;
using MoonDiary.Presentation;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.ViewModels;
using MoonDiary.Services;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace MoonDiary.Controls;

public sealed class BottomBar : ContentView
{
    private const string CalendarIcon =
        "M5,5 H19 A2,2 0 0 1 21,7 V19 A2,2 0 0 1 19,21 H5 A2,2 0 0 1 3,19 V7 A2,2 0 0 1 5,5 Z M3,10 H21 M8,3 V7 M16,3 V7";

    private const string AnalyticsIcon = "M4,20.5 H20 M7,17 V12 M12,17 V5 M17,17 V9";

    private const string LettersIcon =
        "M4,6 H20 A1,1 0 0 1 21,7 V17 A1,1 0 0 1 20,18 H4 A1,1 0 0 1 3,17 V7 A1,1 0 0 1 4,6 Z M3.5,7.5 L12,13.5 L20.5,7.5";

    private const string UserIcon =
        "M16,8 A4,4 0 1 1 8,8 A4,4 0 1 1 16,8 Z M4,20.5 C4,16.5 8,15 12,15 C16,15 20,16.5 20,20.5";

    private readonly List<(AppTab Tab, Path Icon, Label Title)> items = [];
    private AppTab activeTab = AppTab.Calendar;

    public BottomBar(BottomBarViewModel viewModel)
    {
        var tabs = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            ],
            Padding = new Thickness(4, 6, 4, 6)
        };
        var fill = new Button
        {
            Text = "+",
            Style = Palette.Style("FillButton"),
            AutomationId = "TabFill",
            Command = viewModel.FillCommand,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(6, 0)
        };
        var divider = new BoxView { Style = Palette.Style("Divider") };
        var root = new Grid
        {
            RowDefinitions = [new RowDefinition(1), new RowDefinition(GridLength.Auto)]
        };

        SemanticProperties.SetDescription(fill, Strings.Tab_Fill);
        tabs.Add(CreateItem(viewModel, AppTab.Calendar, Strings.Tab_Calendar, CalendarIcon), 0);
        tabs.Add(CreateItem(viewModel, AppTab.Analytics, Strings.Tab_Analytics, AnalyticsIcon), 1);
        tabs.Add(fill, 2);
        tabs.Add(CreateItem(viewModel, AppTab.Letters, Strings.Tab_Letters, LettersIcon), 3);
        tabs.Add(CreateItem(viewModel, AppTab.User, Strings.Tab_User, UserIcon), 4);
        root.Add(divider);
        root.Add(tabs, 0, 1);
        Palette.Bind(root, BackgroundColorProperty, "Surface");
        Content = root;
        Paint();
    }

    public AppTab ActiveTab
    {
        get => activeTab;
        set
        {
            activeTab = value;
            Paint();
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
            Paint();
        }
    }

    private Grid CreateItem(BottomBarViewModel viewModel, AppTab tab, string title, string iconData)
    {
        var icon = new Path
        {
            Data = (Geometry)new PathGeometryConverter().ConvertFromInvariantString(iconData)!,
            StrokeThickness = 1.8,
            StrokeLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round,
            WidthRequest = 24,
            HeightRequest = 24,
            HorizontalOptions = LayoutOptions.Center,
            InputTransparent = true
        };
        var label = new Label
        {
            Text = title,
            FontSize = 12,
            HorizontalTextAlignment = TextAlignment.Center,
            LineBreakMode = LineBreakMode.NoWrap,
            InputTransparent = true
        };
        var item = new Grid
        {
            RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)],
            RowSpacing = 3,
            Padding = new Thickness(0, 5),
            MinimumHeightRequest = 52,
            VerticalOptions = LayoutOptions.Center,
            AutomationId = "Tab" + tab
        };
        var tap = new TapGestureRecognizer
        {
            Command = viewModel.GoToCommand,
            CommandParameter = tab.ToString()
        };

        item.Add(icon);
        item.Add(label, 0, 1);
        item.GestureRecognizers.Add(tap);
        items.Add((tab, icon, label));

        return item;
    }

    private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e) => Paint();

    private void Paint()
    {
        foreach (var (tab, icon, title) in items)
        {
            var color = tab == activeTab ? Palette.Energy : Palette.TextSecondary;

            icon.Stroke = new SolidColorBrush(color);
            title.TextColor = color;
            title.FontFamily = tab == activeTab ? "OpenSansSemibold" : "OpenSansRegular";
        }
    }
}
