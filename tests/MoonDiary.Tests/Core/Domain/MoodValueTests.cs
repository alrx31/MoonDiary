using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Core.Domain;

public class MoodValueTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void IsValid_OutOfRange_IsFalse(int value)
    {
        Assert.False(MoodValue.IsValid(value));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void IsValid_ValueInRange_IsTrue(int value)
    {
        Assert.True(MoodValue.IsValid(value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void EnsureValid_OutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoodValue.EnsureValid(value, nameof(value)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void EnsureValid_ValueInRange_ReturnsValue(int value)
    {
        Assert.Equal(value, MoodValue.EnsureValid(value, nameof(value)));
    }
}
