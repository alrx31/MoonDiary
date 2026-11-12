namespace MoonDiary.Core.Time;

public sealed class ShiftedTimeProvider : TimeProvider
{
    private readonly TimeProvider baseProvider;
    private readonly DateTimeOffset startUtc;
    private readonly DateTimeOffset baseStartUtc;
    private readonly TimeZoneInfo zone;

    public ShiftedTimeProvider(DateTimeOffset startLocal, TimeProvider? baseProvider = null)
    {
        this.baseProvider = baseProvider ?? System;
        startUtc = startLocal.ToUniversalTime();
        baseStartUtc = this.baseProvider.GetUtcNow();
        zone = TimeZoneInfo.CreateCustomTimeZone("MoonDiaryTest", startLocal.Offset, "MoonDiaryTest", "MoonDiaryTest");
    }

    public override TimeZoneInfo LocalTimeZone => zone;

    public override DateTimeOffset GetUtcNow() => startUtc + (baseProvider.GetUtcNow() - baseStartUtc);
}
