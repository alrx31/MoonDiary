using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Entries;

public sealed class CheckinService
{
    private readonly IEntryRepository entries;
    private readonly DayClock clock;

    public CheckinService(IEntryRepository entries, DayClock clock)
    {
        this.entries = entries;
        this.clock = clock;
    }

    public async Task<Entry> CreateAsync(int energy, int positivity, string? text)
    {
        var now = clock.UtcNow;
        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Type = EntryType.Checkin,
            DayKey = clock.CurrentDayKey,
            Energy = EnsureMood(energy),
            Positivity = EnsureMood(positivity),
            CheckinText = NormalizeText(text),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            TimezoneOffsetMinutes = clock.LocalOffsetMinutes
        };

        await entries.SaveAsync(entry);

        return entry;
    }

    public async Task<Entry> UpdateAsync(Guid id, int energy, int positivity, string? text)
    {
        var entry = await GetCheckinAsync(id);

        entry.Energy = EnsureMood(energy);
        entry.Positivity = EnsureMood(positivity);
        entry.CheckinText = NormalizeText(text);
        entry.UpdatedAtUtc = clock.UtcNow;

        await entries.SaveAsync(entry);

        return entry;
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetCheckinAsync(id);
        await entries.DeleteAsync(id);
    }

    public async Task<Entry> GetAsync(Guid id) => await GetCheckinAsync(id);

    private async Task<Entry> GetCheckinAsync(Guid id)
    {
        var entry = await entries.GetByIdAsync(id) ?? throw new DomainException("Чек-ин не найден");

        if (entry.Type != EntryType.Checkin)
        {
            throw new DomainException("Это не чек-ин");
        }

        return entry;
    }

    private static int EnsureMood(int value) =>
        MoodValue.IsValid(value) ? value : throw new DomainException("Выберите энергию и позитив");

    private static string? NormalizeText(string? text)
    {
        var trimmed = text?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > TextLimits.CheckinText)
        {
            throw new DomainException($"Текст чек-ина не длиннее {TextLimits.CheckinText} символов");
        }

        return trimmed;
    }
}
