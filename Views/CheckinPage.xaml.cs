using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class CheckinPage : ViewModelPage
{
    public CheckinPage(CheckinViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
