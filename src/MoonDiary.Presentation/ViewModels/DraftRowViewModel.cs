using MoonDiary.Core.Entries;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class DraftRowViewModel
{
    public DraftRowViewModel(DraftListItem item)
    {
        DayKey = item.DayKey;
        Text = $"{Ru.DayMonth(item.DayKey)} · " +
               (item.OnlyMood
                   ? Strings.Drafts_OnlyMood
                   : Ru.Count(item.FilledCards, "карточка", "карточки", "карточек"));
    }

    public DateOnly DayKey { get; }

    public string Text { get; }
}
