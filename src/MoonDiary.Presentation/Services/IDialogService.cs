namespace MoonDiary.Presentation.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);

    Task AlertAsync(string title, string message);

    Task<string?> ChooseAsync(string title, IReadOnlyList<string> options, string? destructive = null);
}
