using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class SummaryPage : ViewModelPage
{
    private readonly SummaryViewModel viewModel;

    public SummaryPage(SummaryViewModel viewModel)
        : base(viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        viewModel.AutoClose = () => MainThread.BeginInvokeOnMainThread(() => viewModel.DoneCommand.Execute(null));
    }

    protected override bool OnBackButtonPressed()
    {
        viewModel.DoneCommand.Execute(null);

        return true;
    }
}
