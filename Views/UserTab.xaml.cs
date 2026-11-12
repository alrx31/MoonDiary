using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public partial class UserTab : TabView
{
    private readonly UserViewModel viewModel;

    public UserTab(UserViewModel viewModel)
        : base(viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
    }

    private void OnNameUnfocused(object? sender, FocusEventArgs e) => viewModel.SaveNameCommand.Execute(null);

    private void OnNameCompleted(object? sender, EventArgs e) => NameEntry.Unfocus();
}
