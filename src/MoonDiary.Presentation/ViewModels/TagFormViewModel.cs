using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Tags;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class TagFormViewModel : ViewModelBase
{
    private static readonly LifeDirection[] DirectionValues = Enum.GetValues<LifeDirection>();

    private readonly TagService tags;
    private CardType cardType = CardType.Done;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string label = string.Empty;

    [ObservableProperty]
    private string icon = "⭐";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DirectionName))]
    private int directionIndex = DirectionValues.Length - 1;

    public TagFormViewModel(ViewModelContext context, TagService tags)
        : base(context)
    {
        this.tags = tags;
    }

    public IReadOnlyList<string> Icons { get; } =
    [
        "⭐", "💼", "📚", "🏃", "🍳", "🧹", "🤝", "🛒", "🚶", "📖", "🛋️", "✅", "💡", "💪", "🎯", "🎵",
        "☕", "🎮", "🌿", "🧘", "😴", "😊", "😟", "❤️"
    ];

    public IReadOnlyList<string> Directions { get; } = DirectionValues.Select(LifeDirectionNames.Get).ToList();

    public string DirectionName => Directions[Math.Clamp(DirectionIndex, 0, Directions.Count - 1)];

    public bool CanSave => !string.IsNullOrWhiteSpace(Label);

    public override Task InitializeAsync(IReadOnlyDictionary<string, object> parameters)
    {
        cardType = parameters.GetEnum<CardType>(NavParams.CardType) ?? CardType.Done;
        Label = parameters.GetString(NavParams.Label) ?? string.Empty;

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void SelectIcon(string value) => Icon = value;

    [RelayCommand]
    private async Task PickDirectionAsync()
    {
        var choice = await Dialogs.ChooseAsync(Strings.TagForm_Direction, Directions);
        var index = choice is null ? -1 : Directions.ToList().IndexOf(choice);

        if (index >= 0)
        {
            DirectionIndex = index;
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!CanSave)
        {
            return;
        }

        var direction = DirectionValues[Math.Clamp(DirectionIndex, 0, DirectionValues.Length - 1)];

        if (await RunAsync(async () => await tags.AddAsync(cardType, Label, Icon, direction)))
        {
            await Navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private Task CloseAsync() => Navigation.GoBackAsync();
}
