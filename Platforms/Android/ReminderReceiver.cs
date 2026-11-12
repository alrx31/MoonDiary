using Android.App;
using Android.Content;
using Android.Util;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Letters;
using MoonDiary.Core.Time;
using MoonDiary.Presentation;
using MoonDiary.Presentation.Services;

namespace MoonDiary;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class ReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null)
        {
            return;
        }

        var result = GoAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await HandleAsync(context, intent);
            }
            catch (Exception exception)
            {
                Log.Error("MoonDiary", exception.ToString());
            }
            finally
            {
                result?.Finish();
            }
        });
    }

    private static async Task HandleAsync(Context context, Intent intent)
    {
        var services = IPlatformApplication.Current?.Services;

        if (services is null)
        {
            return;
        }

        await services.GetRequiredService<IAppBootstrapper>().InitializeAsync();

        if (intent.Action == AndroidNotificationService.ReminderAction)
        {
            var clock = services.GetRequiredService<DayClock>();
            var entries = services.GetRequiredService<IEntryRepository>();

            if (!await entries.HasEveningAsync(clock.CurrentDayKey))
            {
                AndroidNotificationService.Show(
                    context,
                    AndroidNotificationService.ReminderNotificationId,
                    AndroidNotificationService.ReminderChannel,
                    Strings.Notification_ReminderTitle,
                    Strings.Notification_ReminderText,
                    StartupCoordinator.OpenWizard);
            }
        }
        else if (intent.Action == AndroidNotificationService.LetterAction
                 && Guid.TryParse(intent.Data?.LastPathSegment, out var letterId))
        {
            var letters = services.GetRequiredService<LetterService>();

            await letters.OpenDueAsync();
            await letters.MarkNotifiedAsync(letterId);
            AndroidNotificationService.Show(
                context,
                AndroidNotificationService.RequestCode(letterId),
                AndroidNotificationService.LetterChannel,
                Strings.Notification_LetterTitle,
                Strings.Notification_LetterText,
                StartupCoordinator.OpenLetterPrefix + letterId.ToString("D"));
        }

        await services.GetRequiredService<ReminderScheduler>().RefreshAsync();
    }
}
