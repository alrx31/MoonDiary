using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class WizardPage : ViewModelPage
{
    private readonly WizardViewModel viewModel;

    public WizardPage(WizardViewModel viewModel)
        : base(viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
    }

    protected override bool OnBack()
    {
        viewModel.BackCommand.Execute(null);

        return true;
    }
}
