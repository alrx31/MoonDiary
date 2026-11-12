using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Tags;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class TagsViewModel : ViewModelBase
{
    private readonly TagService tags;
    private readonly TagSuggestionService suggestions;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDone), nameof(IsAchievement), nameof(IsFeelings), nameof(Title))]
    private CardType cardType = CardType.Done;

    [ObservableProperty]
    private IReadOnlyList<TagRowViewModel> items = [];

    [ObservableProperty]
    private IReadOnlyList<string> suggested = [];

    [ObservableProperty]
    private bool hasSuggestions;

    [ObservableProperty]
    private bool isEmpty;

    public TagsViewModel(ViewModelContext context, TagService tags, TagSuggestionService suggestions)
        : base(context)
    {
        this.tags = tags;
        this.suggestions = suggestions;
    }

    public bool IsDone => CardType == CardType.Done;

    public bool IsAchievement => CardType == CardType.Achievement;

    public bool IsFeelings => CardType == CardType.Feelings;

    public string Title => CardType switch
    {
        CardType.Achievement => Strings.Wizard_AchievementTitle,
        CardType.Feelings => Strings.Wizard_FeelingsTitle,
        _ => Strings.Wizard_DoneTitle
    };

    public override async Task RefreshAsync()
    {
        try
        {
            Items = (await tags.GetManageListAsync(CardType)).Select(tag => new TagRowViewModel(tag)).ToList();
            IsEmpty = Items.Count == 0;
            Suggested = await suggestions.GetSuggestionsAsync(CardType);
            HasSuggestions = Suggested.Count > 0;
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(TagsViewModel), exception);
        }
    }

    [RelayCommand]
    private Task SelectCardAsync(string card)
    {
        CardType = Enum.Parse<CardType>(card);

        return RefreshAsync();
    }

    [RelayCommand]
    private Task AddAsync() => Navigation.PushAsync(Routes.TagForm, NavParams.Of((NavParams.CardType, CardType)));

    [RelayCommand]
    private Task AcceptSuggestionAsync(string phrase) =>
        Navigation.PushAsync(
            Routes.TagForm,
            NavParams.Of((NavParams.CardType, CardType), (NavParams.Label, phrase)));

    [RelayCommand]
    private async Task DismissSuggestionAsync(string phrase)
    {
        await RunAsync(() => suggestions.DismissAsync(phrase));
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task ArchiveAsync(TagRowViewModel row)
    {
        if (!await Dialogs.ConfirmAsync(
                Strings.Common_Delete,
                string.Format(CultureInfo.InvariantCulture, Strings.Tags_ArchiveConfirm, row.Label),
                Strings.Common_Delete,
                Strings.Common_Cancel))
        {
            return;
        }

        await RunAsync(() => tags.ArchiveAsync(row.Id));
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task MoveUpAsync(TagRowViewModel row)
    {
        await RunAsync(() => tags.MoveAsync(row.Id, -1));
        await RefreshAsync();
    }

    [RelayCommand]
    private async Task MoveDownAsync(TagRowViewModel row)
    {
        await RunAsync(() => tags.MoveAsync(row.Id, 1));
        await RefreshAsync();
    }

    [RelayCommand]
    private Task BackAsync() => Navigation.GoBackAsync();
}
