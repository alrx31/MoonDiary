using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class DraftBuilder
{
    private readonly Draft draft;

    private DraftBuilder(DateOnly day)
    {
        draft = new Draft
        {
            DayKey = day,
            ContentJson = "{}",
            UpdatedAtUtc = BuilderTime.EveningUtc(day)
        };
    }

    public static DraftBuilder For(DateOnly day) => new(day);

    public DraftBuilder WithContent(string contentJson)
    {
        draft.ContentJson = contentJson;

        return this;
    }

    public DraftBuilder UpdatedAt(DateTime utc)
    {
        draft.UpdatedAtUtc = utc;

        return this;
    }

    public Draft Build() => draft;
}
