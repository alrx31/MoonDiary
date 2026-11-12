using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Insights;
using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Presentation.ViewModels;

public sealed partial class UserViewModel : ViewModelBase
{
    private readonly ISettingsRepository settings;
    private readonly IDraftRepository drafts;
    private readonly InsightsService insights;
    private string savedName = string.Empty;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string draftsTitle = Strings.User_Drafts;

    [ObservableProperty]
    private string streak = string.Empty;

    public UserViewModel(
        ViewModelContext context,
        ISettingsRepository settings,
        IDraftRepository drafts,
        InsightsService insights)
        : base(context)
    {
        this.settings = settings;
        this.drafts = drafts;
        this.insights = insights;
    }

    public override async Task RefreshAsync()
    {
        try
        {
            var count = await drafts.CountAsync();

            savedName = (await settings.GetAsync()).UserName ?? string.Empty;
            UserName = savedName;
            DraftsTitle = count == 0 ? Strings.User_Drafts : $"{Strings.User_Drafts} ({count})";
            Streak = $"{Strings.Analytics_Streak}: {AnalyticsViewModel.StreakText(await insights.GetStreakAsync())}";
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            ErrorLog.Write(nameof(UserViewModel), exception);
        }
    }

    [RelayCommand]
    public async Task SaveNameAsync()
    {
        var name = UserName.Trim();

        if (name == savedName)
        {
            return;
        }

        await RunAsync(async () =>
        {
            var current = await settings.GetAsync();
            current.UserName = name.Length == 0 ? null : name;
            await settings.SaveAsync(current);
            savedName = name;
        });
    }

    [RelayCommand]
    private Task OpenDraftsAsync() => Navigation.PushAsync(Routes.Drafts);

    [RelayCommand]
    private Task OpenTagsAsync() => Navigation.PushAsync(Routes.Tags);

    [RelayCommand]
    private Task OpenSettingsAsync() => Navigation.PushAsync(Routes.Settings);
}
