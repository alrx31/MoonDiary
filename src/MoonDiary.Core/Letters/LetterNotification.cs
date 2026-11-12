namespace MoonDiary.Core.Letters;

public sealed record LetterNotification(Guid LetterId, DateTimeOffset NotifyAtLocal);
