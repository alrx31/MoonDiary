namespace MoonDiary.Tests.Support.Builders;

internal static class BuilderTime
{
    public const int MinskOffsetMinutes = 180;

    public static DateTime Utc(DateOnly day, TimeOnly time) =>
        new DateTimeOffset(day.ToDateTime(time), TimeSpan.FromMinutes(MinskOffsetMinutes)).UtcDateTime;

    public static DateTime EveningUtc(DateOnly day) => Utc(day, new TimeOnly(20, 0));
}
