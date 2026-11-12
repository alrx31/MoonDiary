using MoonDiary.Core.Domain;
using MoonDiary.Core.Tags;

namespace MoonDiary.Core.Wizard;

public sealed class EveningWizardState
{
    private readonly Dictionary<CardType, string> texts = new()
    {
        [CardType.Done] = string.Empty,
        [CardType.Achievement] = string.Empty,
        [CardType.Feelings] = string.Empty
    };

    private readonly Dictionary<CardType, List<Guid>> selectedTags = new()
    {
        [CardType.Done] = [],
        [CardType.Achievement] = [],
        [CardType.Feelings] = []
    };

    public EveningWizardState(DateOnly dayKey)
    {
        DayKey = dayKey;
    }

    public DateOnly DayKey { get; }

    public Guid? ExistingEntryId { get; private set; }

    public int? Energy { get; private set; }

    public int? Positivity { get; private set; }

    public WizardStep Step { get; private set; } = WizardStep.Mood;

    public bool CanProceed => Energy is not null && Positivity is not null;

    public bool IsLastStep => Step == WizardStep.Feelings;

    public bool HasProgress =>
        Energy is not null || Positivity is not null || texts.Values.Any(text => !string.IsNullOrWhiteSpace(text));

    public int FilledCardsCount =>
        (Energy is not null || Positivity is not null ? 1 : 0)
        + texts.Values.Count(text => !string.IsNullOrWhiteSpace(text));

    public void SetEnergy(int value) => Energy = MoodValue.EnsureValid(value, nameof(value));

    public void SetPositivity(int value) => Positivity = MoodValue.EnsureValid(value, nameof(value));

    public bool Next()
    {
        if (!CanProceed || IsLastStep)
        {
            return false;
        }

        Step++;

        return true;
    }

    public bool Skip() => Step != WizardStep.Mood && Next();

    public bool Back()
    {
        if (Step == WizardStep.Mood)
        {
            return false;
        }

        Step--;

        return true;
    }

    public void GoTo(WizardStep step)
    {
        if (step == WizardStep.Mood || CanProceed)
        {
            Step = step;
        }
    }

    public string GetText(CardType cardType) => texts[cardType];

    public void SetText(CardType cardType, string? text)
    {
        var value = text ?? string.Empty;
        texts[cardType] = value.Length > TextLimits.CardText ? value[..TextLimits.CardText] : value;
    }

    public int RemainingChars(CardType cardType) => TextLimits.CardText - texts[cardType].Length;

    public bool ShowCounter(CardType cardType) => RemainingChars(cardType) < TextLimits.CounterThreshold;

    public IReadOnlyCollection<Guid> GetSelectedTagIds(CardType cardType) => selectedTags[cardType];

    public void ToggleTag(CardType cardType, Tag tag)
    {
        var wasSelected = IsTagSelected(cardType, tag);
        var updated = TagTextComposer.Toggle(texts[cardType], tag.Label);

        if (!wasSelected && updated == texts[cardType])
        {
            return;
        }

        texts[cardType] = updated;
        selectedTags[cardType].Remove(tag.Id);

        if (!wasSelected)
        {
            selectedTags[cardType].Add(tag.Id);
        }
    }

    public bool IsTagSelected(CardType cardType, Tag tag) =>
        selectedTags[cardType].Contains(tag.Id) && TagTextComposer.Contains(texts[cardType], tag.Label);

    public DraftContent ToDraftContent() => new()
    {
        Energy = Energy,
        Positivity = Positivity,
        Done = NullIfEmpty(texts[CardType.Done]),
        Achievement = NullIfEmpty(texts[CardType.Achievement]),
        Feelings = NullIfEmpty(texts[CardType.Feelings]),
        DoneTagIds = [.. selectedTags[CardType.Done]],
        AchievementTagIds = [.. selectedTags[CardType.Achievement]],
        FeelingsTagIds = [.. selectedTags[CardType.Feelings]],
        CurrentStep = (int)Step
    };

    public static EveningWizardState FromDraft(DateOnly dayKey, DraftContent content)
    {
        var state = new EveningWizardState(dayKey);

        if (content.Energy is { } energy && MoodValue.IsValid(energy))
        {
            state.Energy = energy;
        }

        if (content.Positivity is { } positivity && MoodValue.IsValid(positivity))
        {
            state.Positivity = positivity;
        }

        state.SetText(CardType.Done, content.Done);
        state.SetText(CardType.Achievement, content.Achievement);
        state.SetText(CardType.Feelings, content.Feelings);
        state.selectedTags[CardType.Done].AddRange(content.DoneTagIds);
        state.selectedTags[CardType.Achievement].AddRange(content.AchievementTagIds);
        state.selectedTags[CardType.Feelings].AddRange(content.FeelingsTagIds);

        var step = Enum.IsDefined((WizardStep)content.CurrentStep) ? (WizardStep)content.CurrentStep : WizardStep.Mood;
        state.Step = state.CanProceed ? step : WizardStep.Mood;

        return state;
    }

    public static EveningWizardState FromEntry(Entry entry, IReadOnlyCollection<EntryTag> entryTags)
    {
        var state = new EveningWizardState(entry.DayKey)
        {
            ExistingEntryId = entry.Id,
            Energy = entry.Energy,
            Positivity = entry.Positivity
        };

        state.SetText(CardType.Done, entry.Done);
        state.SetText(CardType.Achievement, entry.Achievement);
        state.SetText(CardType.Feelings, entry.Feelings);

        foreach (var link in entryTags)
        {
            state.selectedTags[link.CardType].Add(link.TagId);
        }

        return state;
    }

    public void AttachExistingEntry(Guid entryId) => ExistingEntryId = entryId;

    private static string? NullIfEmpty(string text) => string.IsNullOrWhiteSpace(text) ? null : text;
}
