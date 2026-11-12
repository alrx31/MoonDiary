using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class DatabaseErrorPage : ViewModelPage
{
    public DatabaseErrorPage(DatabaseErrorViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
