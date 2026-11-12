using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class OnboardingPage : ViewModelPage
{
    public OnboardingPage(OnboardingViewModel viewModel)
        : base(viewModel)
    {
        InitializeComponent();
    }
}
