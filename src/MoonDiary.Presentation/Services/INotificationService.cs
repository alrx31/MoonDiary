namespace MoonDiary.Presentation.Services;

public interface INotificationService
{
    Task<bool> IsPermissionGrantedAsync();

    Task<bool> RequestPermissionAsync();

    void ScheduleReminder(DateTimeOffset? at);

    void ScheduleLetter(Guid letterId, DateTimeOffset at);

    void CancelLetter(Guid letterId);

    void OpenNotificationSettings();
}
