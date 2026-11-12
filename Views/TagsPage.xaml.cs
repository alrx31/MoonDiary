using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class TagsPage : ViewModelPage
{
    public TagsPage(TagsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
