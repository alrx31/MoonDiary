using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class ComparisonRowViewModel
{
    public ComparisonRowViewModel(string name, double value, double delta)
    {
        Name = name;
        Arrow = delta >= 0 ? "▲" : "▼";
        Value = $"{Ru.Number(value)} ({Ru.WithSign(delta)})";
    }

    public string Name { get; }

    public string Arrow { get; }

    public string Value { get; }
}
