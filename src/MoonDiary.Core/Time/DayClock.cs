namespace MoonDiary.Core.Time;

public sealed class DayClock
{
    public static readonly TimeOnly DayStart = new(4, 0);
    public static readonly TimeOnly FillWindowStart = new(18, 0);

    private readonly TimeProvider timeProvider;

    public DayClock(TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public DateTimeOffset LocalNow => timeProvider.GetLocalNow();

    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public int LocalOffsetMinutes => (int)LocalNow.Offset.TotalMinutes;

    public DateOnly CurrentDayKey => GetDayKey(LocalNow);

    public bool IsInFillWindow => IsInFillWindowAt(LocalNow);

    public static DateOnly GetDayKey(DateTimeOffset local)
    {
        var date = DateOnly.FromDateTime(local.DateTime);
        var time = TimeOnly.FromDateTime(local.DateTime);

        return time < DayStart ? date.AddDays(-1) : date;
    }

    public static bool IsInFillWindowAt(DateTimeOffset local)
    {
        var time = TimeOnly.FromDateTime(local.DateTime);

        return time >= FillWindowStart || time < DayStart;
    }
}
