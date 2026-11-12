using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class LetterBuilder
{
    private readonly Letter letter;

    private LetterBuilder(DateOnly openDate)
    {
        letter = new Letter
        {
            Id = Guid.NewGuid(),
            Text = "Письмо себе",
            CreatedAtUtc = BuilderTime.EveningUtc(openDate.AddDays(-1)),
            OpenDate = openDate
        };
    }

    public static LetterBuilder For(DateOnly openDate) => new(openDate);

    public LetterBuilder WithText(string text)
    {
        letter.Text = text;

        return this;
    }

    public LetterBuilder CreatedAt(DateTime utc)
    {
        letter.CreatedAtUtc = utc;

        return this;
    }

    public LetterBuilder OpenedAt(DateTime utc)
    {
        letter.OpenedAtUtc = utc;

        return this;
    }

    public LetterBuilder Notified()
    {
        letter.Notified = true;

        return this;
    }

    public LetterBuilder WithId(Guid id)
    {
        letter.Id = id;

        return this;
    }

    public Letter Build() => letter;
}
