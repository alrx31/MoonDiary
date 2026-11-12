using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class AnalyticsTab : TabView
{
    public AnalyticsTab(AnalyticsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
