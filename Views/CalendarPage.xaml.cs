using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class CalendarPage : ViewModelPage
{
    public CalendarPage(CalendarViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
