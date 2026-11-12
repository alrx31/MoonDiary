using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Core.Domain;

public class DomainEnumsTests
{
    [Theory]
    [InlineData(typeof(EntryType), 2)]
    [InlineData(typeof(CardType), 3)]
    [InlineData(typeof(LifeDirection), 7)]
    [InlineData(typeof(MoodScale), 2)]
    [InlineData(typeof(ReportStatus), 2)]
    [InlineData(typeof(AdviceFeedback), 2)]
    [InlineData(typeof(ThemeMode), 3)]
    public void GetValues_Enum_HasExpectedValueCount(Type enumType, int expectedCount)
    {
        var values = Enum.GetValues(enumType);

        Assert.Equal(expectedCount, values.Length);
    }
}
