using MoonDiary.Presentation.ViewModels;

namespace MoonDiary.Views;

public abstract class ViewModelPage : AppPage, IQueryAttributable
{
    private readonly ViewModelBase viewModel;
    private IReadOnlyDictionary<string, object> parameters = new Dictionary<string, object>();
    private bool initialized;

    protected ViewModelPage(ViewModelBase viewModel)
    {
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!initialized)
        {
            parameters = new Dictionary<string, object>(query);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!initialized)
        {
            initialized = true;
            await viewModel.InitializeAsync(parameters);
        }

        await viewModel.RefreshAsync();
    }
}
