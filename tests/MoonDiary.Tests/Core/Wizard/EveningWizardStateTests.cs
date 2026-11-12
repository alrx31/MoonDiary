using MoonDiary.Core.Domain;
using MoonDiary.Core.Wizard;
using MoonDiary.Tests.Support;
using MoonDiary.Tests.Support.Builders;

namespace MoonDiary.Tests.Core.Wizard;

public class EveningWizardStateTests
{
    private static readonly DateOnly Day = Dates.D(2026, 11, 10);

    [Fact]
    public void NewState_CannotProceedUntilBothScalesChosen()
    {
        var state = new EveningWizardState(Day);

        Assert.False(state.CanProceed);
        state.SetEnergy(4);
        Assert.False(state.CanProceed);
        Assert.False(state.Next());
        state.SetPositivity(3);
        state.SetPositivity(3);

        Assert.True(state.CanProceed);
        Assert.Equal(3, state.Positivity);
    }

    [Fact]
    public void Next_GoesToLastStepAndStops()
    {
        var state = Ready();

        Assert.True(state.Next());
        Assert.True(state.Next());
        Assert.True(state.Next());
        Assert.Equal(WizardStep.Feelings, state.Step);
        Assert.True(state.IsLastStep);
        Assert.False(state.Next());
    }

    [Fact]
    public void Skip_OnMoodStep_IsNotAllowed()
    {
        var state = Ready();

        Assert.False(state.Skip());
        state.Next();
        Assert.True(state.Skip());
        Assert.Equal(WizardStep.Achievement, state.Step);
    }

    [Fact]
    public void Back_OnFirstStep_ReturnsFalse()
    {
        var state = Ready();
        state.Next();
        state.Next();

        Assert.True(state.Back());
        Assert.Equal(WizardStep.Done, state.Step);
        Assert.True(state.Back());
        Assert.False(state.Back());
    }

    [Fact]
    public void SetText_LongText_IsTruncatedAndCounterShown()
    {
        var state = new EveningWizardState(Day);

        state.SetText(CardType.Done, new string('а', 600));
        Assert.Equal(500, state.GetText(CardType.Done).Length);

        state.SetText(CardType.Done, new string('а', 451));
        Assert.True(state.ShowCounter(CardType.Done));
        Assert.Equal(49, state.RemainingChars(CardType.Done));

        state.SetText(CardType.Done, new string('а', 450));
        Assert.False(state.ShowCounter(CardType.Done));
    }

    [Fact]
    public void ToggleTag_Twice_SelectsAndClears()
    {
        var state = new EveningWizardState(Day);
        var tag = TagBuilder.For(CardType.Done, "спорт").Build();

        state.ToggleTag(CardType.Done, tag);
        Assert.True(state.IsTagSelected(CardType.Done, tag));
        Assert.Equal("спорт", state.GetText(CardType.Done));

        state.ToggleTag(CardType.Done, tag);
        Assert.False(state.IsTagSelected(CardType.Done, tag));
        Assert.Equal(string.Empty, state.GetText(CardType.Done));
    }

    [Fact]
    public void IsTagSelected_LabelErasedByHand_ReturnsFalse()
    {
        var state = new EveningWizardState(Day);
        var tag = TagBuilder.For(CardType.Done, "спорт").Build();
        state.ToggleTag(CardType.Done, tag);

        state.SetText(CardType.Done, "отдых");

        Assert.False(state.IsTagSelected(CardType.Done, tag));
    }

    [Fact]
    public void HasProgress_AndFilledCards_ReflectInput()
    {
        var state = new EveningWizardState(Day);
        Assert.False(state.HasProgress);

        state.SetEnergy(2);
        state.SetText(CardType.Feelings, "устал");

        Assert.True(state.HasProgress);
        Assert.Equal(2, state.FilledCardsCount);
    }

    [Fact]
    public void Draft_RoundTrip_RestoresValuesAndStep()
    {
        var state = Ready();
        var tag = TagBuilder.For(CardType.Done, "спорт").Build();
        state.Next();
        state.ToggleTag(CardType.Done, tag);
        state.Next();

        var json = DraftContent.Serialize(state.ToDraftContent());
        var restored = EveningWizardState.FromDraft(Day, DraftContent.Deserialize(json));

        Assert.Equal(4, restored.Energy);
        Assert.Equal(3, restored.Positivity);
        Assert.Equal("спорт", restored.GetText(CardType.Done));
        Assert.True(restored.IsTagSelected(CardType.Done, tag));
        Assert.Equal(WizardStep.Achievement, restored.Step);
    }

    [Fact]
    public void DraftContent_BrokenJson_Throws()
    {
        Assert.Throws<System.Text.Json.JsonException>(() => DraftContent.Deserialize("{oops"));
    }

    [Fact]
    public void FromEntry_LoadsSavedValuesOnMoodStep()
    {
        var entry = EntryBuilder.Evening(Day).WithMood(5, 4).WithDone("спорт").Build();
        var tag = TagBuilder.For(CardType.Done, "спорт").Build();

        var state = EveningWizardState.FromEntry(entry, [new EntryTag(entry.Id, tag.Id, CardType.Done)]);

        Assert.Equal(entry.Id, state.ExistingEntryId);
        Assert.Equal(WizardStep.Mood, state.Step);
        Assert.Equal(5, state.Energy);
        Assert.True(state.IsTagSelected(CardType.Done, tag));
    }

    private static EveningWizardState Ready()
    {
        var state = new EveningWizardState(Day);
        state.SetEnergy(4);
        state.SetPositivity(3);

        return state;
    }
}
