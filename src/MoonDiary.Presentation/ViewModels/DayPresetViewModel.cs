using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class DayPresetViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isSelected;

    public DayPresetViewModel(int days)
    {
        Days = days;
        Text = days.ToString(CultureInfo.InvariantCulture);
        AutomationId = "NewLetterPreset" + Text;
    }

    public int Days { get; }

    public string Text { get; }

    public string AutomationId { get; }
}
