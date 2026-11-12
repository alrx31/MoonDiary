using MoonDiary.Core.Domain;
using MoonDiary.Core.Resources;

namespace MoonDiary.Tests.Core.Resources;

public class StarterTagsTests
{
    [Theory]
    [InlineData(CardType.Done)]
    [InlineData(CardType.Achievement)]
    [InlineData(CardType.Feelings)]
    public void All_EachCard_HasTenDefinitions(CardType cardType)
    {
        Assert.Equal(10, StarterTags.All.Count(definition => definition.CardType == cardType));
    }

    [Fact]
    public void All_Loaded_HasThirtyDefinitions()
    {
        Assert.Equal(30, StarterTags.All.Count);
    }

    [Fact]
    public void All_Labels_HaveNoMoreThanThreeWords()
    {
        Assert.All(StarterTags.All, definition =>
        {
            var words = definition.Label.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Assert.InRange(words.Length, 1, 3);
        });
    }

    [Fact]
    public void All_Labels_UniqueWithinCard()
    {
        var duplicates = StarterTags.All
            .GroupBy(definition => (definition.CardType, definition.Label))
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Theory]
    [InlineData("одиноко", LifeDirection.Relations)]
    [InlineData("сонно", LifeDirection.Sleep)]
    [InlineData("чтение", LifeDirection.None)]
    [InlineData("сдал задачу", LifeDirection.Work)]
    public void All_Label_HasExpectedDirection(string label, LifeDirection direction)
    {
        var definition = Assert.Single(StarterTags.All, item => item.Label == label);

        Assert.Equal(direction, definition.Direction);
    }

    [Fact]
    public void CreateTags_Called_OrdersFromZeroToNineWithinCard()
    {
        var tags = StarterTags.CreateTags();

        Assert.Equal(30, tags.Count);

        foreach (var group in tags.GroupBy(tag => tag.CardType))
        {
            Assert.Equal(Enumerable.Range(0, 10), group.Select(tag => tag.Order).Order());
        }
    }
}
