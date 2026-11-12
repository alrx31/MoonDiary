using CommunityToolkit.Mvvm.Input;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class BottomBarViewModel
{
    private readonly INavigationService navigation;

    public BottomBarViewModel(INavigationService navigation)
    {
        this.navigation = navigation;
    }

    [RelayCommand]
    private Task GoToAsync(string tab) => navigation.GoToTabAsync(Enum.Parse<AppTab>(tab));

    [RelayCommand]
    private Task FillAsync() => navigation.PushAsync(Routes.Wizard);
}
