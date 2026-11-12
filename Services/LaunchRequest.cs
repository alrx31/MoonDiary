using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class LaunchRequest : ILaunchRequest
{
    public const string OpenExtra = "moondiary_open_target";

    private static string? pending;

    public static void Set(string? target)
    {
        if (!string.IsNullOrWhiteSpace(target))
        {
            pending = target;
        }
    }

    public string? TakeOpenTarget() => Interlocked.Exchange(ref pending, null);
}
