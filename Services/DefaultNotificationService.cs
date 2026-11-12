using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class DefaultNotificationService : INotificationService
{
    public Task<bool> IsPermissionGrantedAsync() => Task.FromResult(true);

    public Task<bool> RequestPermissionAsync() => Task.FromResult(true);

    public void ScheduleReminder(DateTimeOffset? at)
    {
    }

    public void ScheduleLetter(Guid letterId, DateTimeOffset at)
    {
    }

    public void CancelLetter(Guid letterId)
    {
    }

    public void OpenNotificationSettings()
    {
    }
}
