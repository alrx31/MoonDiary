using MoonDiary.Core.Domain;
using MoonDiary.Core.Tags;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Core.Tags;

public class TagTextComposerTests
{
    [Theory]
    [InlineData("", "работа", "работа")]
    [InlineData(null, "работа", "работа")]
    [InlineData("работа", "спорт", "работа, спорт")]
    [InlineData("был в зале,  ", "спорт", "был в зале, спорт")]
    public void Append_AddsLabelWithSeparator(string? text, string label, string expected)
    {
        Assert.Equal(expected, TagTextComposer.Append(text, label));
    }

    [Fact]
    public void Append_OverLimit_KeepsText()
    {
        var text = new string('а', 495);

        Assert.Equal(text, TagTextComposer.Append(text, "работа"));
    }

    [Theory]
    [InlineData("работа, спорт, уборка", "спорт", "работа, уборка")]
    [InlineData("работа, спорт", "работа", "спорт")]
    [InlineData("работа", "работа", "")]
    public void Remove_DropsLabelAndExtraCommas(string text, string label, string expected)
    {
        Assert.Equal(expected, TagTextComposer.Remove(text, label));
    }

    [Fact]
    public void Toggle_Twice_ReturnsOriginalText()
    {
        var once = TagTextComposer.Toggle("был в зале", "спорт");

        Assert.Equal("был в зале, спорт", once);
        Assert.Equal("был в зале", TagTextComposer.Toggle(once, "спорт"));
    }

    [Fact]
    public void Contains_IsCaseSensitiveSubstring()
    {
        Assert.False(TagTextComposer.Contains("Спорт", "спорт"));
        Assert.True(TagTextComposer.Contains("немного спорта", "спорт"));
        Assert.False(TagTextComposer.Contains(null, "спорт"));
    }

    [Fact]
    public void ResolveLinkedTagIds_KeepsOnlyLabelsPresentInText()
    {
        var work = TagBuilder.For(CardType.Done, "работа").Build();
        var sport = TagBuilder.For(CardType.Done, "спорт").Build();

        var linked = TagTextComposer.ResolveLinkedTagIds("спорт и отдых", [work, sport, sport]);

        Assert.Equal([sport.Id], linked);
    }
}
