using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Tests.Support.Fakes;

public sealed class FakeNavigationService : INavigationService
{
    public List<(string Method, string? Route, IReadOnlyDictionary<string, object>? Params)> Calls { get; } = [];

    public (string Method, string? Route, IReadOnlyDictionary<string, object>? Params) Last => Calls[^1];

    public Task GoToTabAsync(AppTab tab) => Record("tab", tab.ToString(), null);

    public Task GoToRootAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        Record("root", route, parameters);

    public Task PushAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        Record("push", route, parameters);

    public Task ReplaceAsync(string route, IReadOnlyDictionary<string, object>? parameters = null) =>
        Record("replace", route, parameters);

    public Task GoBackAsync() => Record("back", null, null);

    private Task Record(string method, string? route, IReadOnlyDictionary<string, object>? parameters)
    {
        Calls.Add((method, route, parameters));

        return Task.CompletedTask;
    }
}
