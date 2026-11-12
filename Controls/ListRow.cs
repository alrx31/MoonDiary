using System.Windows.Input;

namespace MoonDiary.Controls;

public sealed class ListRow : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(ListRow), string.Empty,
        propertyChanged: (view, _, value) => ((ListRow)view).title.Text = (string)value);

    public static readonly BindableProperty DetailProperty = BindableProperty.Create(
        nameof(Detail), typeof(string), typeof(ListRow), string.Empty,
        propertyChanged: (view, _, value) => ((ListRow)view).SetDetail((string?)value));

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(ListRow));

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(ListRow));

    private readonly Label title = new() { VerticalOptions = LayoutOptions.Center, LineBreakMode = LineBreakMode.TailTruncation };
    private readonly Label detail = new() { VerticalOptions = LayoutOptions.Center, IsVisible = false };

    public ListRow()
    {
        var chevron = new Label { Text = "›", VerticalOptions = LayoutOptions.Center };
        var grid = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            ],
            ColumnSpacing = 12,
            MinimumHeightRequest = 52
        };

        detail.SetDynamicResource(StyleProperty, "SecondaryLabel");
        chevron.SetDynamicResource(StyleProperty, "MutedLabel");
        grid.Add(title, 0);
        grid.Add(detail, 1);
        grid.Add(chevron, 2);

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) =>
        {
            if (Command?.CanExecute(CommandParameter) == true)
            {
                Command.Execute(CommandParameter);
            }
        };
        grid.GestureRecognizers.Add(tap);
        Content = grid;
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private void SetDetail(string? value)
    {
        detail.Text = value ?? string.Empty;
        detail.IsVisible = !string.IsNullOrEmpty(value);
    }
}
