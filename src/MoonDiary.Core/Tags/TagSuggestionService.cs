using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Tags;

public sealed class TagSuggestionService
{
    public const int LookbackDays = 60;
    public const int MinEntries = 3;
    public const int MaxPerCard = 3;

    private static readonly char[] Separators = [',', '.', ';', '!', '?', '\n', '\r'];

    private readonly IEntryRepository entries;
    private readonly ITagRepository tags;
    private readonly ISettingsRepository settings;
    private readonly DayClock clock;

    public TagSuggestionService(
        IEntryRepository entries,
        ITagRepository tags,
        ISettingsRepository settings,
        DayClock clock)
    {
        this.entries = entries;
        this.tags = tags;
        this.settings = settings;
        this.clock = clock;
    }

    public async Task<IReadOnlyList<string>> GetSuggestionsAsync(CardType cardType)
    {
        var since = clock.CurrentDayKey.AddDays(-LookbackDays);
        var evenings = await entries.GetEveningsWithTextSinceAsync(since);
        var known = (await tags.GetAllAsync())
            .Where(tag => tag.CardType == cardType)
            .Select(tag => tag.Label.ToLowerInvariant())
            .ToHashSet();
        var dismissed = (await settings.GetAsync()).DismissedTagSuggestions
            .Select(phrase => phrase.ToLowerInvariant())
            .ToHashSet();
        var counts = new Dictionary<string, int>();

        foreach (var entry in evenings)
        {
            var text = cardType switch
            {
                CardType.Done => entry.Done,
                CardType.Achievement => entry.Achievement,
                _ => entry.Feelings
            };

            foreach (var phrase in ExtractPhrases(text).Distinct())
            {
                if (!known.Contains(phrase) && !dismissed.Contains(phrase))
                {
                    counts[phrase] = counts.GetValueOrDefault(phrase) + 1;
                }
            }
        }

        return counts
            .Where(pair => pair.Value >= MinEntries)
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Take(MaxPerCard)
            .Select(pair => pair.Key)
            .ToList();
    }

    public async Task DismissAsync(string phrase)
    {
        var normalized = phrase.Trim().ToLowerInvariant();
        var current = await settings.GetAsync();

        if (!current.DismissedTagSuggestions.Contains(normalized))
        {
            current.DismissedTagSuggestions.Add(normalized);
            await settings.SaveAsync(current);
        }
    }

    private static IEnumerable<string> ExtractPhrases(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        foreach (var part in text.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
        {
            var words = part.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var phrase = string.Join(' ', words).ToLowerInvariant();

            if (words.Length is >= 1 and <= TextLimits.TagLabelMaxWords && phrase.Length >= 3)
            {
                yield return phrase;
            }
        }
    }
}
