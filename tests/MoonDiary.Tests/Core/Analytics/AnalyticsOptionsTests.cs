using MoonDiary.Core.Analytics;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Analytics;

public class AnalyticsOptionsTests
{
    private static readonly Dictionary<string, string> AllValues = new()
    {
        ["n"] = "5", ["e"] = "3,5", ["p"] = "4,0", ["k"] = "2", ["delta"] = "0,8", ["direction"] = "выше",
        ["arrow"] = "▲", ["tag"] = "спорт", ["day"] = "вторник", ["text"] = "сдал задачу",
        ["weekday"] = "в понедельник"
    };

    [Fact]
    public void Default_HasThresholdsFromCatalog()
    {
        var options = AnalyticsOptions.Default;

        Assert.Equal(3, options.MinDaysForReport);
        Assert.Equal(0.8, options.PatternMinDelta);
        Assert.Equal(3, options.AdviceMaxCount);
        Assert.Equal(7, options.WeekdayNames.Count);
        Assert.Equal(7, options.WeekdayPhrases.Count);
    }

    [Fact]
    public void Templates_EveryRule_HasTwoOrThreeRenderableVariants()
    {
        var options = AnalyticsOptions.Default;
        var expected = new[]
        {
            "AR-01", "AR-02", "AR-03", "AR-04-energy", "AR-04-positivity", "AR-05", "AR-10", "AR-11", "AR-12",
            "AR-20", "AR-21", "AR-22", "AR-23", "AR-24", "AR-25", "AR-30", "AR-31", "AR-32", "AR-33", "AR-34",
            "AR-35", "AR-36"
        };

        Assert.Equal(expected.Order(), options.Templates.Keys.Order());

        foreach (var variants in options.Templates.Values)
        {
            Assert.InRange(variants.Count, 2, 3);
            Assert.All(variants, variant => Assert.DoesNotContain("{", TemplateRenderer.Render(variant, AllValues)));
        }
    }

    [Fact]
    public void PickTemplate_NeighbourWeeks_GiveDifferentVariants()
    {
        var options = AnalyticsOptions.Default;
        var week = Dates.D(2026, 11, 9);

        Assert.NotEqual(options.PickTemplate("AR-01", week), options.PickTemplate("AR-01", week.AddDays(7)));
        Assert.Throws<KeyNotFoundException>(() => options.PickTemplate("AR-99", week));
    }

    [Fact]
    public void Render_MissingValue_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => TemplateRenderer.Render("{n} из {k}", ("n", "1")));
    }

    [Fact]
    public void Number_RoundsToOneDecimalInRussianFormat()
    {
        Assert.Equal("4,1", TemplateRenderer.Number(4.06));
        Assert.Equal("4,0", TemplateRenderer.Number(4));
    }
}
