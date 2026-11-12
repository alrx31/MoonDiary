using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class TagFormPage : ViewModelPage
{
    public TagFormPage(TagFormViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
