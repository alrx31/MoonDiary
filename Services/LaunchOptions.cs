using MoonDiary.Presentation.Testing;

namespace MoonDiary.Services;

public static class LaunchOptions
{
    public const string DefaultDatabaseFileName = "moondiary.db";

    public static ParsedLaunchOptions Current { get; set; } = ParsedLaunchOptions.Empty;

    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, Current.DatabaseFileName ?? DefaultDatabaseFileName);

    public static void ReadFromEnvironment()
    {
#if DEBUG
        Current = LaunchOptionsParser.Parse(key => Environment.GetEnvironmentVariable(key.ToUpperInvariant()));
#endif
    }
}
