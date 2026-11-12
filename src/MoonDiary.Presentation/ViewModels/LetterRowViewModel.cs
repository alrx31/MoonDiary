using MoonDiary.Core.Letters;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class LetterRowViewModel
{
    public LetterRowViewModel(LetterListItem item, int index)
    {
        Id = item.Id;
        OpenDate = item.OpenDate;
        IsOpen = item.IsOpen;
        Marker = item.IsOpen ? "✓" : "▸";
        Date = Ru.FullDate(item.OpenDate);
        Status = item.IsOpen ? $"«{item.Preview}»" : Strings.Letters_WillOpen;
        AutomationId = $"LettersRow{index.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }

    public Guid Id { get; }

    public DateOnly OpenDate { get; }

    public bool IsOpen { get; }

    public string Marker { get; }

    public string Date { get; }

    public string Status { get; }

    public string AutomationId { get; }
}
