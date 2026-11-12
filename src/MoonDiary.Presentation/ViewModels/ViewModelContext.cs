using MoonDiary.Presentation.Navigation;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Presentation.ViewModels;

public sealed record ViewModelContext(INavigationService Navigation, IDialogService Dialogs, IErrorLog ErrorLog);
