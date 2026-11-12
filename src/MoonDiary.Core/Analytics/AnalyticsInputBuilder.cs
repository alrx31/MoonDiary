using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Calendar;
using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Analytics;

public sealed class AnalyticsInputBuilder
{
    private readonly IEntryRepository entries;
    private readonly ITagRepository tags;

    public AnalyticsInputBuilder(IEntryRepository entries, ITagRepository tags)
    {
        this.entries = entries;
        this.tags = tags;
    }

    public async Task<AnalyticsInput> BuildAsync(DateOnly weekStart)
    {
        var periodStart = weekStart.AddDays(-21);
        var weekEnd = weekStart.AddDays(6);
        var all = await entries.GetRangeAsync(periodStart, weekEnd);
        var evenings = all.Where(entry => entry.Type == EntryType.Evening).ToDictionary(entry => entry.DayKey);
        var checkins = all.Where(entry => entry.Type == EntryType.Checkin).ToLookup(entry => entry.DayKey);
        var links = (await entries.GetEntryTagsAsync(evenings.Values.Select(entry => entry.Id).ToList()))
            .ToLookup(link => link.EntryId);
        var tagById = (await tags.GetAllAsync()).ToDictionary(tag => tag.Id);
        var period = new List<AnalyticsDay>();

        for (var date = periodStart; date <= weekEnd; date = date.AddDays(1))
        {
            evenings.TryGetValue(date, out var evening);
            var dayCheckins = checkins[date].ToList();

            period.Add(new AnalyticsDay(
                date,
                DayValueCalculator.Calculate(evening, dayCheckins),
                evening,
                dayCheckins,
                evening is null ? [] : links[evening.Id].ToList()));
        }

        return new AnalyticsInput(
            weekStart,
            period.Where(day => day.Date >= weekStart).ToList(),
            period.Where(day => day.Date >= weekStart.AddDays(-7) && day.Date < weekStart).ToList(),
            period,
            tagById);
    }
}
