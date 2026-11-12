using MoonDiary.Core.Domain;

namespace MoonDiary.Presentation.ViewModels;

public sealed class TagRowViewModel
{
    public TagRowViewModel(Tag tag)
    {
        Id = tag.Id;
        Label = tag.Label;
        Display = $"{tag.Icon} {tag.Label}";
        Direction = LifeDirectionNames.Get(tag.Direction);
    }

    public Guid Id { get; }

    public string Label { get; }

    public string Display { get; }

    public string Direction { get; }
}
