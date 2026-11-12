using CommunityToolkit.Mvvm.ComponentModel;
using MoonDiary.Core.Domain;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class AdviceItemViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUseful), nameof(IsNotRelevant))]
    private AdviceFeedback? feedback;

    public AdviceItemViewModel(ReportAdvice advice, int index)
    {
        Id = advice.Id;
        Text = advice.Text;
        Direction = LifeDirectionNames.Get(advice.Direction);
        Number = $"{index + 1}.";
        feedback = advice.Feedback;
    }

    public Guid Id { get; }

    public string Number { get; }

    public string Text { get; }

    public string Direction { get; }

    public bool IsUseful => Feedback == AdviceFeedback.Useful;

    public bool IsNotRelevant => Feedback == AdviceFeedback.NotRelevant;
}
