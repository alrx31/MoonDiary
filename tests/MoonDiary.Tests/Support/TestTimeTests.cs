namespace MoonDiary.Tests.Support;

public class TestTimeTests
{
    [Fact]
    public void GetLocalNow_AtMinskTime_ReturnsLocalTimeWithOffset()
    {
        var time = TestTime.At(2026, 11, 10, 3, 59);

        var now = time.GetLocalNow();

        Assert.Equal(new DateTime(2026, 11, 10, 3, 59, 0), now.DateTime);
        Assert.Equal(TimeSpan.FromHours(3), now.Offset);
    }

    [Fact]
    public void Advance_OneMinute_MovesLocalTimeToNextHour()
    {
        var time = TestTime.At(2026, 11, 10, 3, 59);

        time.Advance(TimeSpan.FromMinutes(1));

        var now = time.GetLocalNow();

        Assert.Equal(new DateTime(2026, 11, 10, 4, 0, 0), now.DateTime);
        Assert.Equal(TimeSpan.FromHours(3), now.Offset);
    }
}
