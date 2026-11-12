using MoonDiary.Presentation.Navigation;

namespace MoonDiary.Services;

public sealed class TabState
{
    public event Action? Changed;

    public AppTab Current { get; private set; } = AppTab.Calendar;

    public void Select(AppTab tab)
    {
        if (Current == tab)
        {
            return;
        }

        Current = tab;
        Changed?.Invoke();
    }
}
