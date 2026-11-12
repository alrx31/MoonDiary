using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using MoonDiary.Presentation.Services;
using MoonDiary.Services;

namespace MoonDiary;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode
                           | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[Register("com.moondiary.app.MainActivity")]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        ReadIntent(Intent, isLaunch: true);
        base.OnCreate(savedInstanceState);
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        ReadIntent(intent, isLaunch: false);

        var startup = IPlatformApplication.Current?.Services.GetService<StartupCoordinator>();

        if (startup is not null)
        {
            _ = startup.HandleOpenRequestAsync();
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        DocumentSaver.OnActivityResult(requestCode, resultCode, data);
    }

    private static void ReadIntent(Intent? intent, bool isLaunch)
    {
        if (intent is null)
        {
            return;
        }

#if DEBUG
        if (isLaunch)
        {
            LaunchOptions.Current =
                MoonDiary.Presentation.Testing.LaunchOptionsParser.Parse(key => intent.GetStringExtra(key));
            LaunchRequest.Set(LaunchOptions.Current.Open);
        }
#endif
        LaunchRequest.Set(intent.GetStringExtra(LaunchRequest.OpenExtra));
    }
}
