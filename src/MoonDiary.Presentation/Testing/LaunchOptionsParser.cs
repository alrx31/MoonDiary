using System.Globalization;

namespace MoonDiary.Presentation.Testing;

public static class LaunchOptionsParser
{
    public const string NowKey = "moondiary_now";
    public const string DatabaseKey = "moondiary_db";
    public const string SeedKey = "moondiary_seed";
    public const string OpenKey = "moondiary_open";

    public static ParsedLaunchOptions Parse(Func<string, string?> getExtra)
    {
        DateTimeOffset? now = DateTimeOffset.TryParse(
            getExtra(NowKey),
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : null;
        var database = getExtra(DatabaseKey);

        if (string.IsNullOrWhiteSpace(database)
            || database.Contains('/', StringComparison.Ordinal)
            || database.Contains('\\', StringComparison.Ordinal)
            || database.Contains("..", StringComparison.Ordinal))
        {
            database = null;
        }

        return new ParsedLaunchOptions(now, database, NullIfEmpty(getExtra(SeedKey)), NullIfEmpty(getExtra(OpenKey)));
    }

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
