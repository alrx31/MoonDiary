using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class DraftsPage : ViewModelPage
{
    public DraftsPage(DraftsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
