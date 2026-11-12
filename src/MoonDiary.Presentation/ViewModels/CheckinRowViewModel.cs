using System.Globalization;
using MoonDiary.Core.Domain;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class CheckinRowViewModel
{
    public CheckinRowViewModel(Entry entry, int index)
    {
        Id = entry.Id;
        Time = Ru.Time(entry.CreatedAtUtc, entry.TimezoneOffsetMinutes);
        Energy = entry.Energy;
        Positivity = entry.Positivity;
        Mood = $"⚡ {entry.Energy.ToString(CultureInfo.InvariantCulture)}   ☺ " +
               entry.Positivity.ToString(CultureInfo.InvariantCulture);
        Text = entry.CheckinText ?? string.Empty;
        AutomationId = $"DayCheckinRow{index.ToString(CultureInfo.InvariantCulture)}";
    }

    public Guid Id { get; }

    public string Time { get; }

    public int Energy { get; }

    public int Positivity { get; }

    public string Mood { get; }

    public string Text { get; }

    public string AutomationId { get; }
}
