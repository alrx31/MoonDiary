using Android.Content;
using MoonDiary.Presentation.Services;

namespace MoonDiary;

public sealed class AndroidBackupSettings : IBackupSettings
{
    public bool IsAutoBackupSupported => OperatingSystem.IsAndroidVersionAtLeast(23);

    public void OpenBackupSettings()
    {
        var context = global::Android.App.Application.Context;

        try
        {
            context.StartActivity(new Intent("android.settings.PRIVACY_SETTINGS").AddFlags(ActivityFlags.NewTask));
        }
        catch (ActivityNotFoundException)
        {
            context.StartActivity(
                new Intent(global::Android.Provider.Settings.ActionSettings).AddFlags(ActivityFlags.NewTask));
        }
    }
}
