using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class ReportPage : ViewModelPage
{
    private readonly ReportViewModel viewModel;

    public ReportPage(ReportViewModel viewModel)
        : base(viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        viewModel.CloseCommand.Execute(null);

        return true;
    }
}
