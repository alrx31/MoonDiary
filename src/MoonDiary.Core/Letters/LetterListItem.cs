namespace MoonDiary.Core.Letters;

public sealed record LetterListItem(Guid Id, DateOnly OpenDate, TimeOnly OpenTime, bool IsOpen, string? Preview);
