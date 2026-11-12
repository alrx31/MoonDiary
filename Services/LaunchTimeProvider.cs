using MoonDiary.Core.Time;

namespace MoonDiary.Services;

public sealed class LaunchTimeProvider : TimeProvider
{
    private readonly Lazy<TimeProvider> inner = new(() =>
        LaunchOptions.Current.Now is { } now ? new ShiftedTimeProvider(now) : System);

    public override TimeZoneInfo LocalTimeZone => inner.Value.LocalTimeZone;

    public override DateTimeOffset GetUtcNow() => inner.Value.GetUtcNow();

    public override long GetTimestamp() => inner.Value.GetTimestamp();

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period) =>
        inner.Value.CreateTimer(callback, state, dueTime, period);
}
