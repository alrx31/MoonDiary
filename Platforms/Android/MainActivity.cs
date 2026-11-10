using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace MoonDiary;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[Register("com.companyname.moondiary.MainActivity")]
public class MainActivity : MauiAppCompatActivity
{
}
