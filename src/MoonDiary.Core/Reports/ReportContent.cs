using System.Text.Json;
using MoonDiary.Core.Analytics;
using MoonDiary.Core.Resources;

namespace MoonDiary.Core.Reports;

public sealed record ReportContent(
    IReadOnlyList<string> Summary,
    IReadOnlyList<string> Achievements,
    IReadOnlyList<string> Patterns,
    WeekComparison? Comparison,
    bool Insufficient)
{
    public static ReportContent Empty { get; } = new([], [], [], null, true);

    public static string Serialize(ReportContent content) =>
        JsonSerializer.Serialize(content, ResourceStore.JsonOptions);

    public static ReportContent Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ReportContent>(json, ResourceStore.JsonOptions) ?? Empty;
        }
        catch (JsonException)
        {
            return Empty;
        }
    }
}
