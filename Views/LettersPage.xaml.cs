using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class LettersPage : ViewModelPage
{
    public LettersPage(LettersViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
