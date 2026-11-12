namespace MoonDiary.Core.Letters;

public sealed record LetterListItem(Guid Id, DateOnly OpenDate, bool IsOpen, string? Preview);
