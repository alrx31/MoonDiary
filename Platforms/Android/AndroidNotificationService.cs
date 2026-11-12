using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using MoonDiary.Presentation.Services;
using MoonDiary.Services;

namespace MoonDiary;

public sealed class AndroidNotificationService : INotificationService
{
    public const string ReminderChannel = "reminder";
    public const string LetterChannel = "letters";
    public const string ReminderAction = "com.moondiary.app.REMINDER";
    public const string LetterAction = "com.moondiary.app.LETTER";
    public const int ReminderNotificationId = 1001;

    private const long WindowMillis = 10 * 60 * 1000;

    private static Context AppContext => global::Android.App.Application.Context;

    public async Task<bool> IsPermissionGrantedAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();

        return status == PermissionStatus.Granted && NotificationManagerCompat.From(AppContext).AreNotificationsEnabled();
    }

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await MainThread.InvokeOnMainThreadAsync(
            () => Permissions.RequestAsync<Permissions.PostNotifications>());

        return status == PermissionStatus.Granted;
    }

    public void ScheduleReminder(DateTimeOffset? at)
    {
        var intent = new Intent(AppContext, typeof(ReminderReceiver));
        intent.SetAction(ReminderAction);
        Schedule(intent, ReminderNotificationId, at);
    }

    public void ScheduleLetter(Guid letterId, DateTimeOffset at) => Schedule(LetterIntent(letterId), RequestCode(letterId), at);

    public void CancelLetter(Guid letterId) => Schedule(LetterIntent(letterId), RequestCode(letterId), null);

    public void OpenNotificationSettings()
    {
        var intent = OperatingSystem.IsAndroidVersionAtLeast(26)
            ? new Intent(global::Android.Provider.Settings.ActionAppNotificationSettings)
                .PutExtra(global::Android.Provider.Settings.ExtraAppPackage, AppContext.PackageName)
            : new Intent(
                global::Android.Provider.Settings.ActionApplicationDetailsSettings,
                global::Android.Net.Uri.Parse("package:" + AppContext.PackageName));

        intent.AddFlags(ActivityFlags.NewTask);
        AppContext.StartActivity(intent);
    }

    public static int RequestCode(Guid letterId) => 2000 + (Math.Abs(letterId.GetHashCode()) % 1_000_000);

    public static void Show(Context context, int id, string channel, string title, string text, string openTarget)
    {
        var manager = NotificationManagerCompat.From(context);

        if (!manager.AreNotificationsEnabled())
        {
            return;
        }

        EnsureChannels(context);

        var open = new Intent(context, typeof(MainActivity));
        open.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);
        open.PutExtra(LaunchRequest.OpenExtra, openTarget);

        var content = PendingIntent.GetActivity(
            context,
            id,
            open,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var notification = new NotificationCompat.Builder(context, channel)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(title)
            .SetContentText(text)
            .SetAutoCancel(true)
            .SetContentIntent(content)
            .SetPriority(NotificationCompat.PriorityDefault)
            .Build();

        if (notification is not null)
        {
            manager.Notify(id, notification);
        }
    }

    private static Intent LetterIntent(Guid letterId)
    {
        var intent = new Intent(AppContext, typeof(ReminderReceiver));
        intent.SetAction(LetterAction);
        intent.SetData(global::Android.Net.Uri.Parse("moondiary://letter/" + letterId.ToString("D")));

        return intent;
    }

    private static void Schedule(Intent intent, int requestCode, DateTimeOffset? at)
    {
        var pending = PendingIntent.GetBroadcast(
            AppContext,
            requestCode,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        if (pending is null || AppContext.GetSystemService(Context.AlarmService) is not AlarmManager alarms)
        {
            return;
        }

        alarms.Cancel(pending);

        if (at is { } moment)
        {
            alarms.SetWindow(AlarmType.RtcWakeup, moment.ToUnixTimeMilliseconds(), WindowMillis, pending);
        }
    }

    private static void EnsureChannels(Context context)
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26)
            || context.GetSystemService(Context.NotificationService) is not NotificationManager manager)
        {
            return;
        }

        manager.CreateNotificationChannel(
            new NotificationChannel(ReminderChannel, "Напоминание о записи", NotificationImportance.Default));
        manager.CreateNotificationChannel(
            new NotificationChannel(LetterChannel, "Письма в будущее", NotificationImportance.Default));
    }
}
