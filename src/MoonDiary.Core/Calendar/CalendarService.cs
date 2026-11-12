using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Calendar;

public sealed class CalendarService
{
    private static readonly CardType[] CardOrder = [CardType.Done, CardType.Achievement, CardType.Feelings];

    private readonly IEntryRepository entries;
    private readonly ITagRepository tags;
    private readonly DayClock clock;

    public CalendarService(IEntryRepository entries, ITagRepository tags, DayClock clock)
    {
        this.entries = entries;
        this.tags = tags;
        this.clock = clock;
    }

    public async Task<CalendarMonth> GetMonthAsync(int year, int month)
    {
        var today = clock.CurrentDayKey;
        var first = new DateOnly(year, month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var monthEntries = await entries.GetRangeAsync(first, last);
        var evenings = monthEntries.Where(entry => entry.Type == EntryType.Evening).ToDictionary(entry => entry.DayKey);
        var checkins = monthEntries.Where(entry => entry.Type == EntryType.Checkin).ToLookup(entry => entry.DayKey);
        var links = await entries.GetEntryTagsAsync(evenings.Values.Select(entry => entry.Id).ToList());
        var linksByEntry = links.ToLookup(link => link.EntryId);
        var tagById = (await tags.GetByIdsAsync(links.Select(link => link.TagId).Distinct().ToList()))
            .ToDictionary(tag => tag.Id);
        var days = new List<CalendarDay>();

        for (var date = first; date <= last; date = date.AddDays(1))
        {
            evenings.TryGetValue(date, out var evening);
            var dayCheckins = checkins[date].ToList();

            days.Add(new CalendarDay(
                date,
                DayValueCalculator.Calculate(evening, dayCheckins),
                evening is not null,
                dayCheckins.Count,
                date == today,
                date > today,
                evening is null ? null : GetShortText(evening, linksByEntry[evening.Id], tagById)));
        }

        var currentMonth = new DateOnly(today.Year, today.Month, 1);

        return new CalendarMonth(year, month, days, first < currentMonth, true);
    }

    public async Task<IReadOnlyList<CalendarDay>> GetFeedAsync(int year, int month)
    {
        var calendarMonth = await GetMonthAsync(year, month);

        return calendarMonth.Days.Where(day => !day.IsFuture).Reverse().ToList();
    }

    public async Task<DayDetails> GetDayAsync(DateOnly date)
    {
        var today = clock.CurrentDayKey;
        var evening = await entries.GetEveningAsync(date);
        var checkins = await entries.GetCheckinsAsync(date);
        IReadOnlyList<Tag> eveningTags = [];

        if (evening is not null)
        {
            var links = await entries.GetEntryTagsAsync([evening.Id]);
            var tagById = (await tags.GetByIdsAsync(links.Select(link => link.TagId).Distinct().ToList()))
                .ToDictionary(tag => tag.Id);

            eveningTags = links
                .Where(link => tagById.ContainsKey(link.TagId))
                .OrderBy(link => Array.IndexOf(CardOrder, link.CardType))
                .ThenBy(link => tagById[link.TagId].Order)
                .Select(link => tagById[link.TagId])
                .ToList();
        }

        return new DayDetails(
            date,
            evening,
            checkins,
            DayValueCalculator.Calculate(evening, checkins),
            eveningTags,
            date == today,
            date > today);
    }

    private static string? GetShortText(Entry evening, IEnumerable<EntryTag> links, Dictionary<Guid, Tag> tagById)
    {
        var achievement = evening.Achievement?.Trim();

        if (!string.IsNullOrEmpty(achievement))
        {
            return achievement.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }

        return links
            .Where(link => tagById.ContainsKey(link.TagId))
            .OrderBy(link => Array.IndexOf(CardOrder, link.CardType))
            .ThenBy(link => tagById[link.TagId].Order)
            .Select(link => tagById[link.TagId].Label)
            .FirstOrDefault();
    }
}
