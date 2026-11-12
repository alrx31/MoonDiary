using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Entries;
using MoonDiary.Core.Tags;
using MoonDiary.Core.Time;
using MoonDiary.Core.Wizard;
using MoonDiary.Presentation.Formatting;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class WizardViewModel : ViewModelBase
{
    private readonly EntryService entries;
    private readonly TagService tags;
    private readonly DayClock clock;
    private readonly ReminderScheduler scheduler;
    private EveningWizardState state;
    private Guid? reportAfter;
    private bool dirty;
    private bool loading;

    [ObservableProperty]
    private string title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMoodStep), nameof(IsTextStep), nameof(CardTitle))]
    private int step = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnergyLabel), nameof(CanProceed))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand), nameof(SaveCommand))]
    private int energy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositivityLabel), nameof(CanProceed))]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand), nameof(SaveCommand))]
    private int positivity;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string counter = string.Empty;

    [ObservableProperty]
    private bool showCounter;

    [ObservableProperty]
    private bool hasDraftLink;

    public WizardViewModel(
        ViewModelContext context,
        EntryService entries,
        TagService tags,
        DayClock clock,
        ReminderScheduler scheduler)
        : base(context)
    {
        this.entries = entries;
        this.tags = tags;
        this.clock = clock;
        this.scheduler = scheduler;
        state = new EveningWizardState(clock.CurrentDayKey);
    }

    public ObservableCollection<TagChipViewModel> Tags { get; } = [];

    public bool IsMoodStep => Step == 1;

    public bool IsTextStep => Step > 1;

    public bool CanProceed => Energy > 0 && Positivity > 0;

    public string CardTitle => Step switch
    {
        2 => Strings.Wizard_DoneTitle,
        3 => Strings.Wizard_AchievementTitle,
        4 => Strings.Wizard_FeelingsTitle,
        _ => Strings.Wizard_MoodTitle
    };

    public string EnergyLabel => Energy > 0 ? MoodLabels.Get(MoodScale.Energy, Energy) : Strings.Wizard_ChooseScale;

    public string PositivityLabel =>
        Positivity > 0 ? MoodLabels.Get(MoodScale.Positivity, Positivity) : Strings.Wizard_ChooseScale;

    public override async Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        var dayKey = parameters.GetDate(NavParams.DayKey) ?? clock.CurrentDayKey;
        reportAfter = parameters.GetGuid(NavParams.ReportId);

        try
        {
            var draft = parameters.GetBool(NavParams.FromDraft) ? await entries.OpenDraftAsync(dayKey) : null;
            state = draft ?? await entries.OpenWizardAsync(dayKey);
            HasDraftLink = draft is null && await entries.HasDraftAsync(dayKey);
        }
        catch (DomainException exception)
        {
            await Dialogs.AlertAsync(Strings.Common_Error, exception.Message);
            await Navigation.GoBackAsync();

            return;
        }

        Title = Ru.DayTitle(state.DayKey);
        await LoadStepAsync();
    }

    [RelayCommand(CanExecute = nameof(CanProceed))]
    private async Task ContinueAsync()
    {
        if (!CanProceed)
        {
            return;
        }

        if (state.Next())
        {
            await LoadStepAsync();
        }
        else
        {
            await SaveAsync();
        }
    }

    [RelayCommand]
    private async Task SkipAsync()
    {
        if (IsMoodStep || !CanProceed)
        {
            return;
        }

        if (state.Skip())
        {
            await LoadStepAsync();
        }
        else
        {
            await SaveAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanProceed))]
    private async Task SaveAsync()
    {
        if (!CanProceed)
        {
            return;
        }

        var saved = await RunAsync(async () => await entries.SaveEveningAsync(state));

        if (!saved)
        {
            return;
        }

        await scheduler.RefreshAsync();
        await Navigation.ReplaceAsync(
            Routes.WizardSummary,
            NavParams.Of((NavParams.DayKey, state.DayKey), (NavParams.ReportId, reportAfter)));
    }

    [RelayCommand]
    private async Task BackAsync()
    {
        if (state.Back())
        {
            await LoadStepAsync();
        }
        else
        {
            await CloseAsync();
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        if (dirty)
        {
            await RunAsync(async () => await entries.SaveDraftAsync(state));
        }

        if (reportAfter is { } reportId)
        {
            await Navigation.ReplaceAsync(
                Routes.Report,
                NavParams.Of((NavParams.ReportId, reportId), (NavParams.Auto, true)));
        }
        else
        {
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task OpenDraftAsync()
    {
        var draft = await entries.OpenDraftAsync(state.DayKey);

        if (draft is null)
        {
            HasDraftLink = false;

            return;
        }

        state = draft;
        dirty = true;
        HasDraftLink = false;
        await LoadStepAsync();
    }

    [RelayCommand]
    private void ToggleTag(TagChipViewModel chip)
    {
        if (WizardSteps.ToCardType(state.Step) is not { } card)
        {
            return;
        }

        state.ToggleTag(card, chip.Tag);
        dirty = true;
        loading = true;
        Text = state.GetText(card);
        loading = false;
        RefreshCard(card);
    }

    partial void OnEnergyChanged(int value)
    {
        if (!loading && MoodValue.IsValid(value))
        {
            state.SetEnergy(value);
            dirty = true;
        }
    }

    partial void OnPositivityChanged(int value)
    {
        if (!loading && MoodValue.IsValid(value))
        {
            state.SetPositivity(value);
            dirty = true;
        }
    }

    partial void OnTextChanged(string value)
    {
        if (loading || WizardSteps.ToCardType(state.Step) is not { } card)
        {
            return;
        }

        state.SetText(card, value);
        dirty = true;

        if (state.GetText(card) != value)
        {
            Text = state.GetText(card);
        }

        RefreshCard(card);
    }

    private async Task LoadStepAsync()
    {
        loading = true;
        Energy = state.Energy ?? 0;
        Positivity = state.Positivity ?? 0;
        Step = (int)state.Step;
        Tags.Clear();

        if (WizardSteps.ToCardType(state.Step) is { } card)
        {
            foreach (var tag in await tags.GetSuggestionsForCardAsync(card))
            {
                Tags.Add(new TagChipViewModel(tag));
            }

            Text = state.GetText(card);
            RefreshCard(card);
        }
        else
        {
            Text = string.Empty;
            ShowCounter = false;
        }

        loading = false;
    }

    private void RefreshCard(CardType card)
    {
        foreach (var chip in Tags)
        {
            chip.IsSelected = state.IsTagSelected(card, chip.Tag);
        }

        ShowCounter = state.ShowCounter(card);
        Counter = state.RemainingChars(card).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
