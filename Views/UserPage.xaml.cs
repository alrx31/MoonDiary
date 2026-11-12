using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class UserPage : ViewModelPage
{
    private readonly UserViewModel viewModel;

    public UserPage(UserViewModel viewModel)
        : base(viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
    }

    private void OnNameUnfocused(object? sender, FocusEventArgs e) => viewModel.SaveNameCommand.Execute(null);

    private void OnNameCompleted(object? sender, EventArgs e) => NameEntry.Unfocus();
}
