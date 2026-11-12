using CommunityToolkit.Mvvm.ComponentModel;
using MoonDiary.Core.Domain;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class TagChipViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    public TagChipViewModel(Tag tag)
    {
        Tag = tag;
    }

    public Tag Tag { get; }

    public string Label => Tag.Label;

    public string Icon => Tag.Icon;

    public string Display => $"{Tag.Icon} {Tag.Label}";

    public string AutomationId => $"WizardTag{Tag.Order}";
}
