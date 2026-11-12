using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class ReportAdviceBuilder
{
    private readonly ReportAdvice advice;

    private ReportAdviceBuilder(Guid reportId, string ruleId)
    {
        advice = new ReportAdvice
        {
            Id = Guid.NewGuid(),
            ReportId = reportId,
            RuleId = ruleId,
            Direction = LifeDirection.None,
            Text = "Текст совета"
        };
    }

    public static ReportAdviceBuilder For(Guid reportId, string ruleId) => new(reportId, ruleId);

    public ReportAdviceBuilder WithDirection(LifeDirection direction)
    {
        advice.Direction = direction;

        return this;
    }

    public ReportAdviceBuilder WithText(string text)
    {
        advice.Text = text;

        return this;
    }

    public ReportAdviceBuilder WithFeedback(AdviceFeedback feedback)
    {
        advice.Feedback = feedback;

        return this;
    }

    public ReportAdviceBuilder WithId(Guid id)
    {
        advice.Id = id;

        return this;
    }

    public ReportAdvice Build() => advice;
}
