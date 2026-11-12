using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class DayPage : ViewModelPage
{
    public DayPage(DayViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
