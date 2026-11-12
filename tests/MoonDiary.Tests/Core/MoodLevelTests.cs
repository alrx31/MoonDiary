using MoonDiary.Core;

namespace MoonDiary.Tests.Core;

public class MoodLevelTests
{
    [Fact]
    public void MoodLevel_HasFiveValues_FromOneToFive()
    {
        var values = Enum.GetValues<MoodLevel>();

        Assert.Equal(5, values.Length);
        Assert.Equal(1, (int)MoodLevel.One);
        Assert.Equal(5, (int)MoodLevel.Five);
    }
}
