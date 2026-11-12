using MoonDiary.Core.Resources;

namespace MoonDiary.Tests.Core.Resources;

public class ThanksMessagesTests
{
    [Fact]
    public void All_Loaded_HasAtLeastTenMessages()
    {
        Assert.True(ThanksMessages.All.Count >= 10);
    }

    [Fact]
    public void All_Loaded_HasNoEmptyMessages()
    {
        Assert.All(ThanksMessages.All, message => Assert.False(string.IsNullOrWhiteSpace(message)));
    }

    [Fact]
    public void Pick_AnyRandom_ReturnsMessageFromAll()
    {
        var message = ThanksMessages.Pick(new Random(42));

        Assert.Contains(message, ThanksMessages.All);
    }
}
