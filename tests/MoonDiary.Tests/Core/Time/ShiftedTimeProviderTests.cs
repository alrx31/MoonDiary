using MoonDiary.Core.Time;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Time;

public class ShiftedTimeProviderTests
{
    [Fact]
    public void GetLocalNow_AdvancesFromStartMoment()
    {
        var baseTime = TestTime.At(2026, 1, 1, 9, 0);
        var start = new DateTimeOffset(2026, 11, 10, 18, 0, 0, TimeSpan.FromHours(3));
        var shifted = new ShiftedTimeProvider(start, baseTime);

        baseTime.Advance(TimeSpan.FromSeconds(90));

        Assert.Equal(start.AddSeconds(90), shifted.GetLocalNow());
        Assert.Equal(TimeSpan.FromHours(3), shifted.GetLocalNow().Offset);
        Assert.True(new DayClock(shifted).IsInFillWindow);
    }
}
