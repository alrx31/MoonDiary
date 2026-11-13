using MoonDiary.Controls;
using MoonDiary.Presentation.Navigation;
using MoonDiary.Services;

namespace MoonDiary.Views;

public sealed class HomePage : AppPage
{
    private const double SlideOffset = 18;

    private readonly IServiceProvider services;
    private readonly TabState tabs;
    private readonly Dictionary<AppTab, TabView> views = [];
    private readonly Grid host = new();
    private readonly BottomBar bar;
    private TabView? current;
    private AppTab currentTab;

    public HomePage(IServiceProvider services, TabState tabs, BottomBar bar)
    {
        this.services = services;
        this.tabs = tabs;
        this.bar = bar;

        var root = new Grid
        {
            RowDefinitions = [new RowDefinition(GridLength.Star), new RowDefinition(GridLength.Auto)]
        };

        root.Add(host);
        root.Add(bar, 0, 1);
        Grid.SetRowSpan(bar.FillButton, 2);
        root.Add(bar.FillButton);
        Content = root;
        tabs.Changed += () => _ = ShowAsync(tabs.Current, animate: true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ShowAsync(tabs.Current, animate: false);
    }

    protected override bool OnBack()
    {
        if (tabs.Current == AppTab.Calendar)
        {
            return base.OnBack();
        }

        tabs.Select(AppTab.Calendar);

        return true;
    }

    private async Task ShowAsync(AppTab tab, bool animate)
    {
        var view = GetView(tab);

        bar.ActiveTab = tab;

        if (view == current)
        {
            await view.ActivateAsync();

            return;
        }

        var previous = current;
        var direction = tab > currentTab ? 1 : -1;

        current = view;
        currentTab = tab;

        foreach (var other in views.Values)
        {
            other.CancelAnimations();
            other.IsVisible = other == view || other == previous;
        }

        var activation = view.ActivateAsync();

        if (!animate || previous is null)
        {
            view.Opacity = 1;
            view.TranslationX = 0;

            if (previous is not null)
            {
                previous.IsVisible = false;
            }

            await activation;

            return;
        }

        view.Opacity = 0;
        view.TranslationX = direction * SlideOffset;

        await Task.WhenAll(
            previous.FadeToAsync(0, 110, Easing.CubicIn),
            view.FadeToAsync(1, 200, Easing.CubicOut),
            view.TranslateToAsync(0, 0, 220, Easing.CubicOut));

        if (current == view)
        {
            previous.IsVisible = false;
            view.Opacity = 1;
            view.TranslationX = 0;
        }

        await activation;
    }

    private TabView GetView(AppTab tab)
    {
        if (views.TryGetValue(tab, out var existing))
        {
            return existing;
        }

        TabView view = tab switch
        {
            AppTab.Analytics => services.GetRequiredService<AnalyticsTab>(),
            AppTab.Letters => services.GetRequiredService<LettersTab>(),
            AppTab.User => services.GetRequiredService<UserTab>(),
            _ => services.GetRequiredService<CalendarTab>()
        };

        view.IsVisible = false;
        views[tab] = view;
        host.Add(view);

        return view;
    }
}
