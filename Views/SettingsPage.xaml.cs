using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class SettingsPage : ViewModelPage
{
    public SettingsPage(SettingsViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
