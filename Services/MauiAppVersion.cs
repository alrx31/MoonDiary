using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class MauiAppVersion : IAppVersion
{
    public string VersionString => AppInfo.Current.VersionString;
}
