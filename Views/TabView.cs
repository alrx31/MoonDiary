using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public abstract class TabView : ContentView
{
    private readonly ViewModelBase viewModel;
    private bool initialized;

    protected TabView(ViewModelBase viewModel)
    {
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public async Task ActivateAsync()
    {
        if (!initialized)
        {
            initialized = true;
            await viewModel.InitializeAsync(new Dictionary<string, object>());
        }

        await viewModel.RefreshAsync();
    }
}
