using MoonDiary.Core.Analytics;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Domain;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Support;

public sealed class AnalyticsInputFactory
{
    private readonly DateOnly weekStart;
    private readonly Dictionary<DateOnly, Entry> evenings = [];
    private readonly Dictionary<DateOnly, List<Entry>> checkins = [];
    private readonly Dictionary<DateOnly, List<EntryTag>> links = [];
    private readonly Dictionary<Guid, Tag> tags = [];

    private AnalyticsInputFactory(DateOnly weekStart)
    {
        this.weekStart = weekStart;
    }

    public static AnalyticsInputFactory Week(DateOnly weekStart, params (int E, int P)?[] days)
    {
        var factory = new AnalyticsInputFactory(weekStart);
        factory.Fill(weekStart, days);

        return factory;
    }

    public AnalyticsInputFactory WithPrevious(params (int E, int P)?[] days)
    {
        Fill(weekStart.AddDays(-7), days);

        return this;
    }

    public AnalyticsInputFactory WithPeriodDay(DateOnly date, int energy, int positivity)
    {
        evenings[date] = EntryBuilder.Evening(date).WithMood(energy, positivity).Build();

        return this;
    }

    public AnalyticsInputFactory WithTag(DateOnly date, Tag tag, CardType cardType)
    {
        tags[tag.Id] = tag;

        if (!evenings.TryGetValue(date, out var evening))
        {
            evening = EntryBuilder.Evening(date).Build();
            evenings[date] = evening;
        }

        if (!links.TryGetValue(date, out var list))
        {
            list = [];
            links[date] = list;
        }

        list.Add(new EntryTag(evening.Id, tag.Id, cardType));

        return this;
    }

    public AnalyticsInputFactory WithAchievement(DateOnly date, string text)
    {
        if (!evenings.TryGetValue(date, out var evening))
        {
            evening = EntryBuilder.Evening(date).Build();
            evenings[date] = evening;
        }

        evening.Achievement = text;

        return this;
    }

    public AnalyticsInputFactory WithCheckin(DateOnly date, int energy, int positivity)
    {
        if (!checkins.TryGetValue(date, out var list))
        {
            list = [];
            checkins[date] = list;
        }

        list.Add(EntryBuilder.Checkin(date).WithMood(energy, positivity).Build());

        return this;
    }

    public AnalyticsInput Build()
    {
        var period = new List<AnalyticsDay>();

        for (var date = weekStart.AddDays(-21); date <= weekStart.AddDays(6); date = date.AddDays(1))
        {
            evenings.TryGetValue(date, out var evening);
            var dayCheckins = checkins.GetValueOrDefault(date) ?? [];

            period.Add(new AnalyticsDay(
                date,
                DayValueCalculator.Calculate(evening, dayCheckins),
                evening,
                dayCheckins,
                links.GetValueOrDefault(date) ?? []));
        }

        return new AnalyticsInput(
            weekStart,
            period.Where(day => day.Date >= weekStart).ToList(),
            period.Where(day => day.Date >= weekStart.AddDays(-7) && day.Date < weekStart).ToList(),
            period,
            tags);
    }

    private void Fill(DateOnly start, (int E, int P)?[] days)
    {
        for (var index = 0; index < days.Length; index++)
        {
            if (days[index] is { } mood)
            {
                WithPeriodDay(start.AddDays(index), mood.E, mood.P);
            }
        }
    }
}
