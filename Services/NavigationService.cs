using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Services;

public sealed class NavigationService : INavigationService
{
    public Task GoToTabAsync(AppTab tab) => GoAsync($"//{TabRoute(tab)}", null);

    public Task GoToRootAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        GoAsync($"//{route}", parameters);

    public Task PushAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        GoAsync(route, parameters);

    public Task ReplaceAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        GoAsync($"../{route}", parameters);

    public Task GoBackAsync() => GoAsync("..", null);

    private static string TabRoute(AppTab tab) => tab switch
    {
        AppTab.Analytics => Routes.Analytics,
        AppTab.Letters => Routes.Letters,
        AppTab.User => Routes.User,
        _ => Routes.Calendar
    };

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
