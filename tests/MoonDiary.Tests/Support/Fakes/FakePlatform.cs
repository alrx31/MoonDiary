using MoonDiary.Core.Domain;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Tests.Support.Fakes;

public sealed class FakePlatform :
    IDialogService, IErrorLog, INotificationService, IThemeService, IAppBootstrapper, ILaunchRequest
{
    public List<string> Alerts { get; } = [];

    public List<Exception> Errors { get; } = [];

    public DateTimeOffset? Reminder { get; private set; }

    public ThemeMode? Theme { get; private set; }

    public bool ConfirmResult { get; set; } = true;

    public bool FailInitialize { get; set; }

    public string? OpenTarget { get; set; }

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel) =>
        Task.FromResult(ConfirmResult);

    public Task AlertAsync(string title, string message)
    {
        Alerts.Add(message);

        return Task.CompletedTask;
    }

    public Task<string?> ChooseAsync(string title, IReadOnlyList<string> options, string? destructive = null) =>
        Task.FromResult<string?>(null);

    public TimeOnly? PickedTime { get; set; }

    public Task<TimeOnly?> PickTimeAsync(string title, TimeOnly current) => Task.FromResult(PickedTime);

    public void Write(string source, Exception exception) => Errors.Add(exception);

    public Task<bool> IsPermissionGrantedAsync() => Task.FromResult(true);

    public Task<bool> RequestPermissionAsync() => Task.FromResult(true);

    public void ScheduleReminder(DateTimeOffset? at) => Reminder = at;

    public void ScheduleLetter(Guid letterId, DateTimeOffset at)
    {
    }

    public void CancelLetter(Guid letterId)
    {
    }

    public void OpenNotificationSettings()
    {
    }

    public void Apply(ThemeMode mode) => Theme = mode;

    public Task InitializeAsync() =>
        FailInitialize ? Task.FromException(new IOException("диск")) : Task.CompletedTask;

    public Task<string?> QuarantineDatabaseAsync() => Task.FromResult<string?>(null);

    public string? TakeOpenTarget()
    {
        var target = OpenTarget;
        OpenTarget = null;

        return target;
    }
}
