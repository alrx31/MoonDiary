using Xunit;
using Xunit.Abstractions;

namespace MoonDiary.UiTests;

public class ScenarioTests : AppiumTest
{
    private const string Evening = "2026-11-10T19:30:00+03:00";

    public ScenarioTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    public void Launch_AtSixPm_OpensWizard() => RunScenario(() =>
    {
        Launch("2026-11-10T18:00:00+03:00");

        Assert.Equal("Вторник, 10 ноября", Find("WizardDayTitle").Text);
    });

    [Fact]
    public void Launch_AtThreeFiftyNine_OpensWizardForPreviousDay() => RunScenario(() =>
    {
        Launch("2026-11-11T03:59:00+03:00");

        Assert.Equal("Вторник, 10 ноября", Find("WizardDayTitle").Text);
    });

    [Fact]
    public void Launch_AtFourAm_OpensCalendar() => RunScenario(() =>
    {
        Launch("2026-11-11T04:00:00+03:00");

        Assert.Equal("Ноябрь 2026", Find("CalendarMonthTitle").Text);
        Assert.False(Exists("WizardDayTitle"));
    });

    [Fact]
    public void Wizard_WithoutBothScales_ButtonsAreDisabled() => RunScenario(() =>
    {
        Launch(Evening);

        Assert.False(Find("WizardContinueButton").Enabled);
        Assert.False(Find("WizardSaveButton").Enabled);

        Tap("WizardEnergy4");
        Assert.False(Find("WizardContinueButton").Enabled);

        Tap("WizardPositivity4");
        Assert.True(Find("WizardContinueButton").Enabled);
        Assert.True(Find("WizardSaveButton").Enabled);
    });

    [Fact]
    public void Wizard_SaveOnFirstCard_ShowsSummaryAndDayInCalendar() => RunScenario(() =>
    {
        Launch(Evening);
        Tap("WizardEnergy4");
        Tap("WizardPositivity3");
        Tap("WizardSaveButton");

        Assert.True(Find("SummaryThanksLabel").Displayed);
        Tap("SummaryDoneButton");

        Assert.True(Find("CalendarFeedRow10").Displayed);
        Tap("CalendarFeedRow10");
        Assert.Contains("4", Find("DayEnergyLabel").Text);
        Assert.Contains("3", Find("DayPositivityLabel").Text);
    });

    [Fact]
    public void Wizard_TagTap_AddsLabelToText() => RunScenario(() =>
    {
        Launch(Evening);
        Tap("WizardEnergy3");
        Tap("WizardPositivity3");
        Tap("WizardContinueButton");
        Tap("WizardTag0");

        Assert.Equal("работа", Find("WizardTextEditor").Text);
    });

    [Fact]
    public void Wizard_SlideToEnd_SkipsCard() => RunScenario(() =>
    {
        Launch(Evening);
        Tap("WizardEnergy3");
        Tap("WizardPositivity3");
        Tap("WizardContinueButton");

        var first = Find("WizardCardTitle").Text;

        Slide("WizardSkipSlider", 1.0);
        Thread.Sleep(1500);

        Assert.NotEqual(first, Find("WizardCardTitle").Text);
    });

    [Fact]
    public void Wizard_ShortSlide_StaysOnCard() => RunScenario(() =>
    {
        Launch(Evening);
        Tap("WizardEnergy3");
        Tap("WizardPositivity3");
        Tap("WizardContinueButton");

        var first = Find("WizardCardTitle").Text;

        Slide("WizardSkipSlider", 0.4);
        Thread.Sleep(1500);

        Assert.Equal(first, Find("WizardCardTitle").Text);
    });

    [Fact]
    public void Tabs_Switch_KeepBottomBarAndShowContent() => RunScenario(() =>
    {
        Launch("2026-11-11T12:00:00+03:00");
        Tap("TabLetters");
        Assert.True(Find("LettersNewButton").Displayed);

        Tap("TabUser");
        Assert.True(Find("UserSettingsRow").Displayed);

        Tap("TabCalendar");
        Assert.True(Find("CalendarMonthTitle").Displayed);
    });

    [Fact]
    public void NewLetter_PresetAndTimeSheet_ChangeOpenMoment() => RunScenario(() =>
    {
        Launch("2026-11-11T12:00:00+03:00");
        Tap("TabLetters");
        Tap("LettersNewButton");
        Tap("NewLetterPreset30");
        Tap("NewLetterDaysPlus");
        Assert.Equal("31 день", Find("NewLetterDaysLabel").Text);

        Tap("NewLetterTimeButton");
        Tap("TimeSheetHourPlus");
        Tap("TimeSheetMinutePlus");
        Tap("SheetDone");
        Thread.Sleep(600);

        Assert.Equal("Откроется 12 декабря 2026, 11:05", Find("NewLetterOpenLabel").Text);
    });

    [Fact]
    public void Calendar_PreviousMonth_ChangesTitle() => RunScenario(() =>
    {
        Launch("2026-11-11T12:00:00+03:00");
        Tap("CalendarPreviousButton");
        Thread.Sleep(600);

        Assert.Equal("Октябрь 2026", Find("CalendarMonthTitle").Text);
    });

    [Fact]
    public void Wizard_Close_CreatesDraft() => RunScenario(() =>
    {
        Launch(Evening);
        Tap("WizardEnergy2");
        Tap("WizardCloseButton");
        Tap("TabFill");

        Assert.True(Find("WizardDraftLink").Displayed);

        Tap("WizardCloseButton");
        Tap("TabUser");
        Tap("UserDraftsRow");
        Assert.True(Find("DraftsRow").Displayed);
    });

    [Fact]
    public void Report_OpensAfterSummary() => RunScenario(() =>
    {
        Launch("2026-11-16T19:00:00+03:00", "report");
        Tap("WizardEnergy4");
        Tap("WizardPositivity4");
        Tap("WizardSaveButton");
        Tap("SummaryDoneButton");

        Assert.Contains("9–15 ноября", Find("ReportTitle").Text);
        Tap("ReportCloseButton");
        Assert.True(Find("CalendarMonthTitle").Displayed);
    });
}
