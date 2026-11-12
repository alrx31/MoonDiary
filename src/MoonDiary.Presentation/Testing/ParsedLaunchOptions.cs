namespace MoonDiary.Presentation.Testing;

public sealed record ParsedLaunchOptions(DateTimeOffset? Now, string? DatabaseFileName, string? Seed, string? Open)
{
    public static ParsedLaunchOptions Empty { get; } = new(null, null, null, null);
}
