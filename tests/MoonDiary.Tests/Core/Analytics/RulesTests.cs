using MoonDiary.Core.Analytics;
using MoonDiary.Core.Domain;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Core.Analytics;

public class RulesTests
{
    private static readonly DateOnly Week = Dates.D(2026, 11, 9);

    private static readonly AnalyticsOptions Options = AnalyticsOptions.Default;

    [Fact]
    public void Summary_TwoDays_IsInsufficient()
    {
        var input = AnalyticsInputFactory.Week(Week, (4, 4), (3, 3)).Build();

        var summary = SummaryRules.Evaluate(input, Options);

        Assert.True(summary.Insufficient);
        Assert.Equal(2, summary.DaysWithEntries);
        Assert.Equal(2, summary.Lines.Count);
        Assert.Null(summary.AverageEnergy);
    }

    [Fact]
    public void Summary_ThreeDays_CountsAveragesAndHighEnergyDays()
    {
        var input = AnalyticsInputFactory.Week(Week, (4, 4), (2, 3), (5, 5)).Build();

        var summary = SummaryRules.Evaluate(input, Options);

        Assert.False(summary.Insufficient);
        Assert.Equal("3,7", TemplateRenderer.Number(summary.AverageEnergy!.Value));
        Assert.Equal("4,0", TemplateRenderer.Number(summary.AveragePositivity!.Value));
        Assert.Equal(2, summary.HighEnergyDays);
        Assert.Null(summary.Comparison);
        Assert.Equal(3, summary.Lines.Count);
    }

    [Fact]
    public void Summary_WithPreviousWeek_AddsComparison()
    {
        var input = AnalyticsInputFactory.Week(Week, (4, 4), (2, 3), (5, 5))
            .WithPrevious((3, 5), (3, 5), (3, 5))
            .Build();

        var summary = SummaryRules.Evaluate(input, Options);

        Assert.Equal(0.67, summary.Comparison!.EnergyDelta, 2);
        Assert.Equal(5, summary.Lines.Count);
        Assert.Contains(summary.Lines, line => line.Contains('▲') && line.Contains("0,7"));
        Assert.Contains(summary.Lines, line => line.Contains('▼') && line.Contains("1,0"));
    }

    [Fact]
    public void Achievements_TextsAndFrequentTag_AreListed()
    {
        var tag = TagBuilder.For(CardType.Achievement, "сдал задачу").WithDirection(LifeDirection.Work).Build();
        var input = AnalyticsInputFactory.Week(Week, (4, 4), (3, 3), (3, 3))
            .WithAchievement(Week.AddDays(1), "закончил проект")
            .WithAchievement(Week.AddDays(4), "дописал отчёт")
            .WithTag(Week, tag, CardType.Achievement)
            .WithTag(Week.AddDays(2), tag, CardType.Achievement)
            .Build();

        var result = AchievementRules.Evaluate(input, Options, 5);

        Assert.Equal(3, result.Lines.Count);
        Assert.Contains("вторник", result.Lines[0]);
        Assert.Contains("закончил проект", result.Lines[0]);
        Assert.Contains("сдал задачу", result.Lines[2]);
        Assert.Equal(tag.Id, result.TopTagId);
    }

    [Fact]
    public void Achievements_Nothing_ReturnsSupportLine()
    {
        var input = AnalyticsInputFactory.Week(Week, (4, 4), (3, 3), (3, 3)).Build();

        var result = AchievementRules.Evaluate(input, Options, 3);

        Assert.Contains("3", Assert.Single(result.Lines));
        Assert.Null(result.TopTagId);
    }

    [Fact]
    public void Patterns_TagWithHigherPositivity_GivesAr20()
    {
        var sport = TagBuilder.For(CardType.Done, "спорт").WithDirection(LifeDirection.Health).Build();
        var factory = Period(15, 3, 3);

        for (var index = 0; index < 5; index++)
        {
            var date = Week.AddDays(-21 + (index * 3));
            factory.WithPeriodDay(date, 3, 5).WithTag(date, sport, CardType.Done);
        }

        var findings = PatternRules.EvaluateTagRules(factory.Build(), Options);

        var finding = Assert.Single(findings, f => f.RuleId == "AR-20");
        Assert.Equal(2.0, finding.Delta, 3);
        Assert.Contains("выше", finding.Text);
        Assert.Contains("2,0", finding.Text);
    }

    [Fact]
    public void Patterns_TooFewDaysOrSmallDelta_GiveNothing()
    {
        var sport = TagBuilder.For(CardType.Done, "спорт").WithDirection(LifeDirection.Health).Build();
        var fewDays = Period(15, 3, 3);
        var smallDelta = Period(16, 3, 3);

        for (var index = 0; index < 3; index++)
        {
            var date = Week.AddDays(-21 + (index * 3));
            fewDays.WithPeriodDay(date, 3, 5).WithTag(date, sport, CardType.Done);
        }

        for (var index = 0; index < 8; index++)
        {
            var date = Week.AddDays(-21 + (index * 2));
            smallDelta.WithPeriodDay(date, 3, index % 2 == 0 ? 4 : 3).WithTag(date, sport, CardType.Done);
        }

        Assert.DoesNotContain(PatternRules.EvaluateTagRules(fewDays.Build(), Options), f => f.RuleId == "AR-20");
        Assert.DoesNotContain(PatternRules.EvaluateTagRules(smallDelta.Build(), Options), f => f.RuleId == "AR-20");
    }

    [Fact]
    public void Patterns_TagWithoutDirection_IsIgnored()
    {
        var reading = TagBuilder.For(CardType.Done, "чтение").Build();
        var factory = Period(15, 3, 3);

        for (var index = 0; index < 5; index++)
        {
            var date = Week.AddDays(-21 + (index * 3));
            factory.WithPeriodDay(date, 5, 5).WithTag(date, reading, CardType.Done);
        }

        Assert.Empty(PatternRules.EvaluateTagRules(factory.Build(), Options));
    }

    [Fact]
    public void Patterns_DayAfterAnxiety_GivesAr22Lower()
    {
        var anxiety = TagBuilder.For(CardType.Feelings, "тревожно").WithDirection(LifeDirection.Emotions).Build();
        var factory = Period(28, 3, 4);

        for (var index = 0; index < 5; index++)
        {
            var date = Week.AddDays(-20 + (index * 4));
            factory.WithTag(date, anxiety, CardType.Feelings).WithPeriodDay(date.AddDays(1), 3, 2);
        }

        var finding = Assert.Single(PatternRules.EvaluateTagRules(factory.Build(), Options), f => f.RuleId == "AR-22");

        Assert.True(finding.Delta < 0);
        Assert.Contains("ниже", finding.Text);
    }

    [Fact]
    public void Patterns_LowMondays_GiveAr23()
    {
        var factory = Period(28, 4, 3);

        for (var week = 0; week < 4; week++)
        {
            factory.WithPeriodDay(Week.AddDays(-21 + (week * 7)), 2, 3);
        }

        var result = PatternRules.Evaluate(factory.Build(), Options);

        Assert.Contains(result.All, f => f.RuleId == "AR-23");
        Assert.Contains(result.Lines, line => line.Contains("в понедельник"));
    }

    [Fact]
    public void Patterns_CheckinsLowerThanEvening_GiveAr24OnlyFromFiveDays()
    {
        var five = Period(10, 3, 4);
        var four = Period(10, 3, 4);

        for (var index = 0; index < 5; index++)
        {
            five.WithCheckin(Week.AddDays(6 - index), 3, 2);

            if (index < 4)
            {
                four.WithCheckin(Week.AddDays(6 - index), 3, 2);
            }
        }

        Assert.Contains(PatternRules.Evaluate(five.Build(), Options).All, f => f.RuleId == "AR-24");
        Assert.DoesNotContain(PatternRules.Evaluate(four.Build(), Options).All, f => f.RuleId == "AR-24");
    }

    [Fact]
    public void Patterns_Nothing_ReturnsAr25Line()
    {
        var result = PatternRules.Evaluate(AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3)).Build(), Options);

        Assert.Empty(result.All);
        Assert.Contains("4 недели", Assert.Single(result.Lines));
    }

    [Fact]
    public void Advice_SleepSignals_GiveAr30()
    {
        var sleepy = TagBuilder.For(CardType.Feelings, "сонно").WithDirection(LifeDirection.Sleep).Build();
        var byTag = AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3))
            .WithTag(Week, sleepy, CardType.Feelings)
            .WithTag(Week.AddDays(1), sleepy, CardType.Feelings)
            .WithTag(Week.AddDays(2), sleepy, CardType.Feelings);
        var twoDays = AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3))
            .WithTag(Week, sleepy, CardType.Feelings)
            .WithTag(Week.AddDays(1), sleepy, CardType.Feelings);
        var byEnergy = AnalyticsInputFactory.Week(Week, (2, 3), (2, 3), (1, 3), (4, 4));

        Assert.Contains(Advice(byTag), a => a.RuleId == "AR-30");
        Assert.DoesNotContain(Advice(twoDays), a => a.RuleId == "AR-30");
        Assert.Contains(Advice(byEnergy), a => a.RuleId == "AR-30");
    }

    [Fact]
    public void Advice_HealthPatternAndAbsence_GiveAr31AndAr32()
    {
        var sport = TagBuilder.For(CardType.Done, "спорт").WithDirection(LifeDirection.Health).Build();
        var factory = Period(28, 3, 3);

        foreach (var offset in new[] { -20, -17, -14, 0, 2 })
        {
            factory.WithPeriodDay(Week.AddDays(offset), 3, 5).WithTag(Week.AddDays(offset), sport, CardType.Done);
        }

        var advice = Advice(factory);
        var planned = Assert.Single(advice, a => a.RuleId == "AR-31");

        Assert.Contains("спорт", planned.Text);
        Assert.Contains("3", planned.Text);
        Assert.DoesNotContain(advice, a => a.RuleId == "AR-32");
        Assert.Contains(Advice(AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3))), a => a.RuleId == "AR-32");
    }

    [Fact]
    public void Advice_WorkEveryDayWithLowPositivity_GivesAr33()
    {
        var work = TagBuilder.For(CardType.Done, "работа").WithDirection(LifeDirection.Work).Build();
        var six = AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3), (3, 3), (3, 2), (3, 3));
        var five = AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3), (3, 3), (3, 2));

        for (var index = 0; index < 6; index++)
        {
            six.WithTag(Week.AddDays(index), work, CardType.Done);

            if (index < 5)
            {
                five.WithTag(Week.AddDays(index), work, CardType.Done);
            }
        }

        Assert.Contains(Advice(six), a => a.RuleId == "AR-33");
        Assert.DoesNotContain(Advice(five), a => a.RuleId == "AR-33");
    }

    [Fact]
    public void Advice_LonelyAndAnxiety_CountDaysOnce()
    {
        var lonely = TagBuilder.For(CardType.Feelings, "одиноко").WithDirection(LifeDirection.Relations).Build();
        var anxiety = TagBuilder.For(CardType.Feelings, "тревожно").WithDirection(LifeDirection.Emotions).Build();
        var irritated = TagBuilder.For(CardType.Feelings, "раздражён").WithDirection(LifeDirection.Emotions).Build();
        var fires = AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3))
            .WithTag(Week, lonely, CardType.Feelings)
            .WithTag(Week.AddDays(1), lonely, CardType.Feelings)
            .WithTag(Week, anxiety, CardType.Feelings)
            .WithTag(Week.AddDays(1), anxiety, CardType.Feelings)
            .WithTag(Week.AddDays(2), irritated, CardType.Feelings);
        var silent = AnalyticsInputFactory.Week(Week, (3, 3), (3, 3), (3, 3))
            .WithTag(Week, lonely, CardType.Feelings)
            .WithTag(Week, anxiety, CardType.Feelings)
            .WithTag(Week, irritated, CardType.Feelings)
            .WithTag(Week.AddDays(1), anxiety, CardType.Feelings);

        Assert.Contains(Advice(fires), a => a.RuleId == "AR-34");
        Assert.Contains(Advice(fires), a => a.RuleId == "AR-35");
        Assert.DoesNotContain(Advice(silent), a => a.RuleId is "AR-34" or "AR-35");
    }

    [Fact]
    public void Advice_HouseholdPatternNextDay_GivesAr36()
    {
        var cleaning = TagBuilder.For(CardType.Done, "уборка").WithDirection(LifeDirection.Household).Build();
        var factory = Period(28, 3, 3);

        for (var index = 0; index < 5; index++)
        {
            var date = Week.AddDays(-20 + (index * 4));
            factory.WithTag(date, cleaning, CardType.Done).WithPeriodDay(date.AddDays(1), 3, 5);
        }

        Assert.Contains("уборка", Assert.Single(Advice(factory), a => a.RuleId == "AR-36").Text);
    }

    [Fact]
    public void Advice_InsufficientData_IsEmpty()
    {
        Assert.Empty(Advice(AnalyticsInputFactory.Week(Week, (1, 1), (1, 1))));
    }

    [Fact]
    public void Selector_PrefersDifferentDirectionsThenStrength()
    {
        var candidates = new List<AdviceCandidate>
        {
            new("AR-30", LifeDirection.Sleep, 0.9, "сон"),
            new("AR-31", LifeDirection.Health, 0.8, "спорт"),
            new("AR-32", LifeDirection.Health, 0.7, "прогулка"),
            new("AR-34", LifeDirection.Relations, 0.3, "звонок"),
            new("AR-35", LifeDirection.Emotions, 0.2, "чек-ин")
        };

        var three = AdviceSelector.Select(candidates, [], 3);
        var twoDirections = AdviceSelector.Select(candidates.Take(3).ToList(), [], 3);
        var excluded = AdviceSelector.Select(candidates, ["AR-30"], 3);

        Assert.Equal(["AR-30", "AR-31", "AR-34"], three.Select(a => a.RuleId));
        Assert.Equal(["AR-30", "AR-31", "AR-32"], twoDirections.Select(a => a.RuleId));
        Assert.DoesNotContain(excluded, a => a.RuleId == "AR-30");
        Assert.Empty(AdviceSelector.Select([], [], 3));
    }

    private static IReadOnlyList<AdviceCandidate> Advice(AnalyticsInputFactory factory)
    {
        var input = factory.Build();

        return AdviceRules.Evaluate(
            input,
            SummaryRules.Evaluate(input, Options),
            PatternRules.Evaluate(input, Options),
            Options);
    }

    private static AnalyticsInputFactory Period(int days, int energy, int positivity)
    {
        var factory = AnalyticsInputFactory.Week(Week);

        for (var index = 0; index < days; index++)
        {
            factory.WithPeriodDay(Week.AddDays(6 - index), energy, positivity);
        }

        return factory;
    }
}
