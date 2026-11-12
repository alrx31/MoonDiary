using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Core.Domain;

public class LifeDirectionNamesTests
{
    [Theory]
    [InlineData(LifeDirection.Sleep, "Сон")]
    [InlineData(LifeDirection.Work, "Работа и учёба")]
    [InlineData(LifeDirection.Health, "Здоровье и спорт")]
    [InlineData(LifeDirection.Relations, "Отношения")]
    [InlineData(LifeDirection.Household, "Быт")]
    [InlineData(LifeDirection.Emotions, "Эмоции")]
    [InlineData(LifeDirection.None, "Без направления")]
    public void Get_Direction_ReturnsName(LifeDirection direction, string expected)
    {
        Assert.Equal(expected, LifeDirectionNames.Get(direction));
    }
}
