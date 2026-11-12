using System.Text.Json;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Tags;
using MoonDiary.Core.Time;
using MoonDiary.Core.Wizard;

namespace MoonDiary.Core.Entries;

public sealed class EntryService
{
    private static readonly CardType[] Cards = [CardType.Done, CardType.Achievement, CardType.Feelings];

    private readonly IEntryRepository entries;
    private readonly ITagRepository tags;
    private readonly IDraftRepository drafts;
    private readonly DayClock clock;

    public EntryService(IEntryRepository entries, ITagRepository tags, IDraftRepository drafts, DayClock clock)
    {
        this.entries = entries;
        this.tags = tags;
        this.drafts = drafts;
        this.clock = clock;
    }

    public async Task<EveningWizardState> OpenWizardAsync(DateOnly dayKey)
    {
        EnsureNotFuture(dayKey);

        var existing = await entries.GetEveningAsync(dayKey);

        if (existing is null)
        {
            return new EveningWizardState(dayKey);
        }

        var links = await entries.GetEntryTagsAsync([existing.Id]);

        return EveningWizardState.FromEntry(existing, links);
    }

    public async Task<Entry> SaveEveningAsync(EveningWizardState state)
    {
        EnsureNotFuture(state.DayKey);

        if (state.Energy is not { } energy || state.Positivity is not { } positivity)
        {
            throw new DomainException("Выберите энергию и позитив");
        }

        var existing = await entries.GetEveningAsync(state.DayKey);
        var now = clock.UtcNow;
        var entry = existing ?? new Entry
        {
            Id = Guid.NewGuid(),
            Type = EntryType.Evening,
            DayKey = state.DayKey,
            CreatedAtUtc = now,
            TimezoneOffsetMinutes = clock.LocalOffsetMinutes,
            IsBackfilled = state.DayKey < clock.CurrentDayKey
        };

        entry.Energy = energy;
        entry.Positivity = positivity;
        entry.Done = Normalize(state.GetText(CardType.Done));
        entry.Achievement = Normalize(state.GetText(CardType.Achievement));
        entry.Feelings = Normalize(state.GetText(CardType.Feelings));
        entry.UpdatedAtUtc = now;

        var previousLinks = existing is null
            ? []
            : (await entries.GetEntryTagsAsync([existing.Id])).Select(link => link.TagId).ToHashSet();
        var links = new List<EntryTag>();

        foreach (var card in Cards)
        {
            var selected = await tags.GetByIdsAsync(state.GetSelectedTagIds(card).ToList());
            var ordered = state.GetSelectedTagIds(card)
                .Select(id => selected.FirstOrDefault(tag => tag.Id == id))
                .OfType<Tag>();

            foreach (var tagId in TagTextComposer.ResolveLinkedTagIds(state.GetText(card), ordered))
            {
                links.Add(new EntryTag(entry.Id, tagId, card));
            }
        }

        await entries.SaveAsync(entry);
        await entries.ReplaceEntryTagsAsync(entry.Id, links);
        await tags.IncrementUseCountAsync(
            links.Select(link => link.TagId).Distinct().Where(id => !previousLinks.Contains(id)).ToList());
        await drafts.DeleteAsync(state.DayKey);
        state.AttachExistingEntry(entry.Id);

        return entry;
    }

    public async Task DeleteEveningAsync(Guid id)
    {
        var entry = await entries.GetByIdAsync(id);

        if (entry is null)
        {
            return;
        }

        if (entry.Type != EntryType.Evening)
        {
            throw new DomainException("Это не вечерняя запись");
        }

        await entries.DeleteAsync(id);
    }

    public async Task<bool> SaveDraftAsync(EveningWizardState state)
    {
        if (!state.HasProgress)
        {
            return false;
        }

        await drafts.SaveAsync(new Draft
        {
            DayKey = state.DayKey,
            ContentJson = DraftContent.Serialize(state.ToDraftContent()),
            UpdatedAtUtc = clock.UtcNow
        });

        return true;
    }

    public async Task<EveningWizardState?> OpenDraftAsync(DateOnly dayKey)
    {
        var draft = await drafts.GetAsync(dayKey);

        if (draft is null)
        {
            return null;
        }

        try
        {
            var state = EveningWizardState.FromDraft(dayKey, DraftContent.Deserialize(draft.ContentJson));
            var existing = await entries.GetEveningAsync(dayKey);

            if (existing is not null)
            {
                state.AttachExistingEntry(existing.Id);
            }

            return state;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public async Task<bool> HasDraftAsync(DateOnly dayKey) => await drafts.GetAsync(dayKey) is not null;

    public Task DeleteDraftAsync(DateOnly dayKey) => drafts.DeleteAsync(dayKey);

    public async Task<IReadOnlyList<DraftListItem>> GetDraftsAsync()
    {
        var result = new List<DraftListItem>();

        foreach (var draft in await drafts.GetAllAsync())
        {
            try
            {
                var content = DraftContent.Deserialize(draft.ContentJson);
                result.Add(new DraftListItem(
                    draft.DayKey,
                    content.FilledCardsCount,
                    content.FilledTextCards == 0,
                    content.CurrentStep));
            }
            catch (JsonException)
            {
                result.Add(new DraftListItem(draft.DayKey, 0, true, 1));
            }
        }

        return result;
    }

    private void EnsureNotFuture(DateOnly dayKey)
    {
        if (dayKey > clock.CurrentDayKey)
        {
            throw new DomainException("Нельзя заполнить будущий день");
        }
    }

    private static string? Normalize(string text)
    {
        var trimmed = text.Trim();

        return trimmed.Length == 0 ? null : trimmed;
    }
}
