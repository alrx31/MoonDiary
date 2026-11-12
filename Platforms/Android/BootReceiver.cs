using Android.App;
using Android.Content;
using Android.Util;
using MoonDiary.Presentation.Services;

namespace MoonDiary;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter([Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced])]
public class BootReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        var services = IPlatformApplication.Current?.Services;

        if (services is null)
        {
            return;
        }

        var result = GoAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await services.GetRequiredService<IAppBootstrapper>().InitializeAsync();
                await services.GetRequiredService<ReminderScheduler>().RefreshAsync();
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
}
