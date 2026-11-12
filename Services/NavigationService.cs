using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Services;

public sealed class NavigationService : INavigationService
{
    private readonly TabState tabs;

    public NavigationService(TabState tabs)
    {
        this.tabs = tabs;
    }

    public Task GoToTabAsync(AppTab tab) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            tabs.Select(tab);

            return Shell.Current.CurrentPage is Views.HomePage
                ? Task.CompletedTask
                : Shell.Current.GoToAsync($"//{Routes.Home}", true);
        });

    public Task GoToRootAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        GoAsync($"//{route}", parameters);

    public Task PushAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        GoAsync(route, parameters);

    public Task ReplaceAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        GoAsync($"../{route}", parameters);

    public Task GoBackAsync() => GoAsync("..", null);

    private static Task GoAsync(string route, IReadOnlyDictionary<string, object>? parameters) =>
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            var query = new ShellNavigationQueryParameters();

            if (parameters is not null)
            {
                foreach (var pair in parameters)
                {
                    query[pair.Key] = pair.Value;
                }
            }

            return Shell.Current.GoToAsync(route, true, query);
        });
}
