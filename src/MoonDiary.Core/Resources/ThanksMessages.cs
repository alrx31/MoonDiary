namespace MoonDiary.Core.Resources;

public static class ThanksMessages
{
    private static readonly Lazy<List<string>> Messages = new(Load);

    public static IReadOnlyList<string> All => Messages.Value;

    public static string Pick(Random random) => All[random.Next(All.Count)];

    private static List<string> Load() =>
        ResourceStore.ReadJson<ThanksFile>("thanks.json").Messages;

    private sealed class ThanksFile
    {
        public List<string> Messages { get; set; } = [];
    }
}
