using MoonDiary.Core.Domain;

namespace MoonDiary.Tests.Support.Builders;

public sealed class EntryBuilder
{
    private readonly Entry entry;

    private EntryBuilder(EntryType type, DateOnly day)
    {
        var createdAt = BuilderTime.EveningUtc(day);

        entry = new Entry
        {
            Id = Guid.NewGuid(),
            Type = type,
            DayKey = day,
            Energy = 3,
            Positivity = 3,
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = createdAt,
            TimezoneOffsetMinutes = BuilderTime.MinskOffsetMinutes
        };
    }

    public static EntryBuilder Evening(DateOnly day) => new(EntryType.Evening, day);

    public static EntryBuilder Checkin(DateOnly day) => new(EntryType.Checkin, day);

    public EntryBuilder WithMood(int energy, int positivity)
    {
        entry.Energy = MoodValue.EnsureValid(energy, nameof(energy));
        entry.Positivity = MoodValue.EnsureValid(positivity, nameof(positivity));

        return this;
    }

    public EntryBuilder WithDone(string done)
    {
        entry.Done = done;

        return this;
    }

    public EntryBuilder WithAchievement(string achievement)
    {
        entry.Achievement = achievement;

        return this;
    }

    public EntryBuilder WithFeelings(string feelings)
    {
        entry.Feelings = feelings;

        return this;
    }

    public EntryBuilder WithCheckinText(string checkinText)
    {
        entry.CheckinText = checkinText;

        return this;
    }

    public EntryBuilder CreatedAt(DateTime utc)
    {
        entry.CreatedAtUtc = utc;
        entry.UpdatedAtUtc = utc;

        return this;
    }

    public EntryBuilder Backfilled()
    {
        entry.IsBackfilled = true;

        return this;
    }

    public EntryBuilder WithId(Guid id)
    {
        entry.Id = id;

        return this;
    }

    public Entry Build() => entry;
}
