using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Core.Domain;

public class MoodLabelsTests
{
    [Theory]
    [InlineData(MoodScale.Energy, 1, "истощение")]
    [InlineData(MoodScale.Energy, 2, "усталость")]
    [InlineData(MoodScale.Energy, 3, "ровно")]
    [InlineData(MoodScale.Energy, 4, "бодрость")]
    [InlineData(MoodScale.Energy, 5, "подъём")]
    [InlineData(MoodScale.Positivity, 1, "плохо")]
    [InlineData(MoodScale.Positivity, 2, "грустно")]
    [InlineData(MoodScale.Positivity, 3, "нейтрально")]
    [InlineData(MoodScale.Positivity, 4, "хорошо")]
    [InlineData(MoodScale.Positivity, 5, "отлично")]
    public void Get_ValueInRange_ReturnsLabel(MoodScale scale, int value, string expected)
    {
        Assert.Equal(expected, MoodLabels.Get(scale, value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Get_OutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MoodLabels.Get(MoodScale.Energy, value));
    }
}
