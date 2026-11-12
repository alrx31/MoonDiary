using MoonDiary.Core.Letters;
using MoonDiary.Core.Reminders;
using MoonDiary.Core.Time;

namespace MoonDiary.Presentation.Services;

public sealed class ReminderScheduler
{
    private readonly ReminderService reminders;
    private readonly LetterService letters;
    private readonly INotificationService notifications;
    private readonly DayClock clock;
    private readonly IErrorLog errorLog;

    public ReminderScheduler(
        ReminderService reminders,
        LetterService letters,
        INotificationService notifications,
        DayClock clock,
        IErrorLog errorLog)
    {
        this.reminders = reminders;
        this.letters = letters;
        this.notifications = notifications;
        this.clock = clock;
        this.errorLog = errorLog;
    }

    public async Task RefreshAsync()
    {
        try
        {
            notifications.ScheduleReminder(await reminders.GetNextReminderAsync());

            foreach (var pending in await letters.GetPendingNotificationsAsync())
            {
                if (pending.NotifyAtLocal <= clock.LocalNow)
                {
                    await letters.MarkNotifiedAsync(pending.LetterId);
                    notifications.CancelLetter(pending.LetterId);
                }
                else
                {
                    notifications.ScheduleLetter(pending.LetterId, pending.NotifyAtLocal);
                }
            }
        }
#pragma warning disable CA1031
        catch (Exception exception)
#pragma warning restore CA1031
        {
            errorLog.Write(nameof(ReminderScheduler), exception);
        }
    }
}
