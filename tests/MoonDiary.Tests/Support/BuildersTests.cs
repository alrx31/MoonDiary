using MoonDiary.Core.Domain;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Support;

public class BuildersTests
{
    private static readonly DateOnly Day = Dates.D(2026, 5, 11);

    private static readonly DateTime EveningUtc = new(2026, 5, 11, 17, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Evening_Default_HasDefaultMood()
    {
        var entry = EntryBuilder.Evening(Day).Build();

        Assert.Equal(EntryType.Evening, entry.Type);
        Assert.Equal(Day, entry.DayKey);
        Assert.Equal(3, entry.Energy);
        Assert.Equal(3, entry.Positivity);
        Assert.True(MoodValue.IsValid(entry.Energy));
        Assert.True(MoodValue.IsValid(entry.Positivity));
    }

    [Fact]
    public void Evening_Default_CreatesAtEightPmLocalTime()
    {
        var entry = EntryBuilder.Evening(Day).Build();

        Assert.Equal(EveningUtc, entry.CreatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, entry.CreatedAtUtc.Kind);
        Assert.Equal(EveningUtc, entry.UpdatedAtUtc);
        Assert.Equal(180, entry.TimezoneOffsetMinutes);
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.False(entry.IsBackfilled);
    }

    [Fact]
    public void Checkin_Default_HasCheckinType()
    {
        var entry = EntryBuilder.Checkin(Day).Build();

        Assert.Equal(EntryType.Checkin, entry.Type);
        Assert.Equal(Day, entry.DayKey);
    }

    [Fact]
    public void WithMood_ValidValues_SetsScales()
    {
        var entry = EntryBuilder.Evening(Day).WithMood(1, 5).Build();

        Assert.Equal(1, entry.Energy);
        Assert.Equal(5, entry.Positivity);
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    [InlineData(6, 3)]
    [InlineData(3, 6)]
    public void WithMood_OutOfRange_Throws(int energy, int positivity)
    {
        var builder = EntryBuilder.Evening(Day);

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMood(energy, positivity));
    }

    [Fact]
    public void Entry_WithTexts_SetsCards()
    {
        var entry = EntryBuilder.Evening(Day)
            .WithDone("сдал задачу")
            .WithAchievement("дошёл до конца")
            .WithFeelings("спокойно")
            .WithCheckinText("хороший день")
            .Build();

        Assert.Equal("сдал задачу", entry.Done);
        Assert.Equal("дошёл до конца", entry.Achievement);
        Assert.Equal("спокойно", entry.Feelings);
        Assert.Equal("хороший день", entry.CheckinText);
    }

    [Fact]
    public void Entry_Modifiers_SetValues()
    {
        var id = Guid.NewGuid();
        var createdAt = new DateTime(2026, 5, 1, 10, 30, 0, DateTimeKind.Utc);

        var entry = EntryBuilder.Evening(Day).WithId(id).CreatedAt(createdAt).Backfilled().Build();

        Assert.Equal(id, entry.Id);
        Assert.Equal(createdAt, entry.CreatedAtUtc);
        Assert.Equal(createdAt, entry.UpdatedAtUtc);
        Assert.True(entry.IsBackfilled);
    }

    [Fact]
    public void Tag_Default_HasCardAndLabel()
    {
        var tag = TagBuilder.For(CardType.Done, "работа").Build();

        Assert.NotEqual(Guid.Empty, tag.Id);
        Assert.Equal(CardType.Done, tag.CardType);
        Assert.Equal("работа", tag.Label);
        Assert.Equal(LifeDirection.None, tag.Direction);
        Assert.Equal(0, tag.Order);
        Assert.Equal(0, tag.UseCount);
        Assert.False(tag.IsCustom);
        Assert.False(tag.IsArchived);
    }

    [Fact]
    public void Tag_Modifiers_SetValues()
    {
        var tag = TagBuilder.For(CardType.Feelings, "одиноко")
            .WithDirection(LifeDirection.Relations)
            .WithOrder(3)
            .WithUseCount(7)
            .Custom()
            .Archived()
            .Build();

        Assert.Equal(LifeDirection.Relations, tag.Direction);
        Assert.Equal(3, tag.Order);
        Assert.Equal(7, tag.UseCount);
        Assert.True(tag.IsCustom);
        Assert.True(tag.IsArchived);
    }

    [Fact]
    public void Draft_Default_HasEmptyJsonAndUtcTime()
    {
        var draft = DraftBuilder.For(Day).Build();

        Assert.Equal(Day, draft.DayKey);
        Assert.Equal("{}", draft.ContentJson);
        Assert.Equal(EveningUtc, draft.UpdatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, draft.UpdatedAtUtc.Kind);
    }

    [Fact]
    public void Draft_WithContent_SetsJson()
    {
        var draft = DraftBuilder.For(Day).WithContent("{\"step\":2}").Build();

        Assert.Equal("{\"step\":2}", draft.ContentJson);
    }

    [Fact]
    public void Letter_Default_IsClosedAndNotNotified()
    {
        var letter = LetterBuilder.For(Day).Build();

        Assert.NotEqual(Guid.Empty, letter.Id);
        Assert.Equal(Day, letter.OpenDate);
        Assert.Equal(new DateTime(2026, 5, 10, 17, 0, 0, DateTimeKind.Utc), letter.CreatedAtUtc);
        Assert.Equal(DateTimeKind.Utc, letter.CreatedAtUtc.Kind);
        Assert.Null(letter.OpenedAtUtc);
        Assert.False(letter.Notified);
    }

    [Fact]
    public void Letter_Opened_SetsOpenTime()
    {
        var openedAt = new DateTime(2026, 5, 12, 6, 0, 0, DateTimeKind.Utc);

        var letter = LetterBuilder.For(Day).WithText("Привет").OpenedAt(openedAt).Notified().Build();

        Assert.Equal("Привет", letter.Text);
        Assert.Equal(openedAt, letter.OpenedAtUtc);
        Assert.True(letter.Notified);
    }

    [Fact]
    public void Report_Default_IsOk()
    {
        var report = TestReportBuilder.For(Day).Build();

        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal(Day, report.WeekStart);
        Assert.Equal(ReportStatus.Ok, report.Status);
        Assert.Equal(7, report.DaysWithEntries);
        Assert.Equal("{}", report.ContentJson);
        Assert.Equal(new DateTime(2026, 5, 11, 6, 0, 0, DateTimeKind.Utc), report.GeneratedAtUtc);
        Assert.Equal(DateTimeKind.Utc, report.GeneratedAtUtc.Kind);
        Assert.Null(report.ShownAtUtc);
    }

    [Fact]
    public void Report_Modifiers_SetValues()
    {
        var shownAt = new DateTime(2026, 5, 18, 6, 0, 0, DateTimeKind.Utc);

        var report = TestReportBuilder.For(Day)
            .WithStatus(ReportStatus.InsufficientData)
            .WithDaysWithEntries(2)
            .WithContent("{\"summary\":\"ok\"}")
            .ShownAt(shownAt)
            .Build();

        Assert.Equal(ReportStatus.InsufficientData, report.Status);
        Assert.Equal(2, report.DaysWithEntries);
        Assert.Equal("{\"summary\":\"ok\"}", report.ContentJson);
        Assert.Equal(shownAt, report.ShownAtUtc);
    }

    [Fact]
    public void ReportAdvice_Default_HasRule()
    {
        var reportId = Guid.NewGuid();

        var advice = ReportAdviceBuilder.For(reportId, "AR-31")
            .WithDirection(LifeDirection.Sleep)
            .WithFeedback(AdviceFeedback.Useful)
            .Build();

        Assert.NotEqual(Guid.Empty, advice.Id);
        Assert.Equal(reportId, advice.ReportId);
        Assert.Equal("AR-31", advice.RuleId);
        Assert.Equal(LifeDirection.Sleep, advice.Direction);
        Assert.Equal(AdviceFeedback.Useful, advice.Feedback);
    }

    [Fact]
    public void EntryTag_Default_LinksEntryAndTag()
    {
        var entryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var entryTag = EntryTagBuilder.For(entryId, tagId, CardType.Done).Build();

        Assert.Equal(entryId, entryTag.EntryId);
        Assert.Equal(tagId, entryTag.TagId);
        Assert.Equal(CardType.Done, entryTag.CardType);
    }
}
