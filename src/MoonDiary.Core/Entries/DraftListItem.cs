namespace MoonDiary.Core.Entries;

public sealed record DraftListItem(DateOnly DayKey, int FilledCards, bool OnlyMood, int CurrentStep);
