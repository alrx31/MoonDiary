using MoonDiary.Core.Letters;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class LetterRowViewModel
{
    public LetterRowViewModel(LetterListItem item, int index)
    {
        Id = item.Id;
        OpenDate = item.OpenDate;
        OpenTime = item.OpenTime;
        IsOpen = item.IsOpen;
        Marker = item.IsOpen ? "✓" : "▸";
        Date = Ru.FullDateTime(item.OpenDate, item.OpenTime);
        Status = item.IsOpen ? $"«{item.Preview}»" : Strings.Letters_WillOpen;
        AutomationId = $"LettersRow{index.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
    }

    public Guid Id { get; }

    public DateOnly OpenDate { get; }

    public TimeOnly OpenTime { get; }

    public bool IsOpen { get; }

    public string Marker { get; }

    public string Date { get; }

    public string Status { get; }

    public string AutomationId { get; }
}
