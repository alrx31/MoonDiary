using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class LettersTab : TabView
{
    public LettersTab(LettersViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
