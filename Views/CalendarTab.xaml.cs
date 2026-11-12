using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class CalendarTab : TabView
{
    public CalendarTab(CalendarViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
