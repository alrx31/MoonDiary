namespace MoonDiary.Presentation.Navigation;

public interface INavigationService
{
    Task GoToTabAsync(AppTab tab);

    Task GoToRootAsync(string route, IReadOnlyDictionary<string, object>? parameters = null);

    Task PushAsync(string route, IReadOnlyDictionary<string, object>? parameters = null);

    Task ReplaceAsync(string route, IReadOnlyDictionary<string, object>? parameters = null);

    Task GoBackAsync();
}
