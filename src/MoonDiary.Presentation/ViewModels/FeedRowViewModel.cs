using System.Globalization;
using MoonDiary.Core.Calendar;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class FeedRowViewModel
{
    public FeedRowViewModel(CalendarDay day)
    {
        Date = day.Date;
        DateText = day.IsToday ? $"{Strings.Calendar_Today}, {Ru.FeedDate(day.Date)}" : Ru.FeedDate(day.Date);
        HasEntry = day.Mood is not null;
        Marker = HasEntry ? "◉" : "○";
        Energy = day.Mood?.Energy ?? 0;
        Positivity = day.Mood?.Positivity ?? 0;
        EnergyText = day.Mood is null ? Strings.Common_NoValue : day.Mood.Energy.ToString(CultureInfo.InvariantCulture);
        PositivityText =
            day.Mood is null ? Strings.Common_NoValue : day.Mood.Positivity.ToString(CultureInfo.InvariantCulture);
        ShortText = HasEntry ? day.ShortText ?? string.Empty : Strings.Calendar_NoEntry;
        AutomationId = $"CalendarFeedRow{day.Date.Day}";
    }

    public DateOnly Date { get; }

    public string DateText { get; }

    public bool HasEntry { get; }

    public string Marker { get; }

    public int Energy { get; }

    public int Positivity { get; }

    public string EnergyText { get; }

    public string PositivityText { get; }

    public string ShortText { get; }

    public string AutomationId { get; }
}
