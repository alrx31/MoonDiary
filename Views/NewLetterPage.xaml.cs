using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class NewLetterPage : ViewModelPage
{
    public NewLetterPage(NewLetterViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
