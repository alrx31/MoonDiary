using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Tags;

public class TagServiceTests
{
    private static readonly DateOnly Today = Dates.D(2026, 11, 10);

    [Fact]
    public async Task Add_CustomTag_GoesToTheEnd()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();

        var tag = await app.TagService.AddAsync(CardType.Done, "  йога  утром ", "🧘", LifeDirection.Health);

        Assert.Equal("йога утром", tag.Label);
        Assert.True(tag.IsCustom);
        Assert.Equal(10, tag.Order);
        Assert.Equal(tag.Id, (await app.TagService.GetManageListAsync(CardType.Done))[^1].Id);
        Assert.Equal(10, (await app.TagService.GetSuggestionsForCardAsync(CardType.Done)).Count);
    }

    [Fact]
    public async Task Add_FourWordsOrDuplicate_IsRejected()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();

        await Assert.ThrowsAsync<DomainException>(
            () => app.TagService.AddAsync(CardType.Done, "раз два три четыре", "•", LifeDirection.None));
        await Assert.ThrowsAsync<DomainException>(
            () => app.TagService.AddAsync(CardType.Done, "Работа", "•", LifeDirection.Work));

        Assert.NotNull(await app.TagService.AddAsync(CardType.Feelings, "работа", "•", LifeDirection.Work));
    }

    [Fact]
    public async Task Rename_KeepsOldEntryTextAndLink()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        var sport = await app.TagAsync(CardType.Done, "спорт");
        var entry = await app.EveningAsync(Today, 3, 3, (CardType.Done, "спорт"));

        await app.TagService.RenameAsync(sport.Id, "зал");

        Assert.Equal("спорт", (await app.Entries.GetByIdAsync(entry.Id))!.Done);
        Assert.Equal(sport.Id, Assert.Single(await app.Entries.GetEntryTagsAsync([entry.Id])).TagId);
        Assert.Equal("зал", (await app.Tags.GetByIdAsync(sport.Id))!.Label);
    }

    [Fact]
    public async Task Archive_HidesFromSuggestionsButKeepsInHistory()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        var sport = await app.TagAsync(CardType.Done, "спорт");
        await app.EveningAsync(Today, 3, 3, (CardType.Done, "спорт"));

        await app.TagService.ArchiveAsync(sport.Id);

        Assert.DoesNotContain(await app.TagService.GetSuggestionsForCardAsync(CardType.Done), tag => tag.Id == sport.Id);
        Assert.Contains((await app.CalendarService.GetDayAsync(Today)).EveningTags, tag => tag.Id == sport.Id);
    }

    [Fact]
    public async Task Move_ChangesOrderWithinBounds()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        var list = await app.TagService.GetManageListAsync(CardType.Done);

        await app.TagService.MoveAsync(list[0].Id, -1);
        Assert.Equal(list[0].Id, (await app.TagService.GetManageListAsync(CardType.Done))[0].Id);

        await app.TagService.MoveAsync(list[0].Id, 1);
        Assert.Equal(list[1].Id, (await app.TagService.GetManageListAsync(CardType.Done))[0].Id);
    }

    [Fact]
    public async Task Suggestions_PhraseInThreeEntries_IsOffered()
    {
        using var app = await TestApp.At(2026, 11, 10, 20, 0).SeedTagsAsync();
        await WriteDoneAsync(app, 0, "Кофе с другом, работа");
        await WriteDoneAsync(app, 1, "кофе с другом. Гулял");
        await WriteDoneAsync(app, 2, "кофе с другом");
        await WriteDoneAsync(app, 3, "гулял");
        await WriteDoneAsync(app, 70, "гулял");

        var suggestions = await app.SuggestionService.GetSuggestionsAsync(CardType.Done);

        Assert.Equal(["кофе с другом"], suggestions);

        await app.SuggestionService.DismissAsync("кофе с другом");
        Assert.Empty(await app.SuggestionService.GetSuggestionsAsync(CardType.Done));
    }

    private static async Task WriteDoneAsync(TestApp app, int daysAgo, string text)
    {
        var state = await app.EntryService.OpenWizardAsync(Today.AddDays(-daysAgo));
        state.SetEnergy(3);
        state.SetPositivity(3);
        state.SetText(CardType.Done, text);
        await app.EntryService.SaveEveningAsync(state);
    }
}
