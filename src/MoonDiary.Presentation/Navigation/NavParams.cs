using System.Globalization;

namespace MoonDiary.Presentation.Navigation;

public static class NavParams
{
    public const string DayKey = "dayKey";
    public const string EntryId = "entryId";
    public const string ReportId = "reportId";
    public const string LetterId = "letterId";
    public const string FromDraft = "fromDraft";
    public const string CardType = "cardType";
    public const string Label = "label";
    public const string Failure = "failure";
    public const string Auto = "auto";

    public static IReadOnlyDictionary<string, object> Of(params (string Key, object? Value)[] values) =>
        values.Where(pair => pair.Value is not null).ToDictionary(pair => pair.Key, pair => pair.Value!);

    public static DateOnly? GetDate(this IReadOnlyDictionary<string, object> parameters, string key) =>
        parameters.TryGetValue(key, out var value)
            ? value switch
            {
                DateOnly date => date,
                string text when DateOnly.TryParseExact(
                    text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) => parsed,
                _ => null
            }
            : null;

    public static Guid? GetGuid(this IReadOnlyDictionary<string, object> parameters, string key) =>
        parameters.TryGetValue(key, out var value)
            ? value switch
            {
                Guid id => id,
                string text when Guid.TryParse(text, out var parsed) => parsed,
                _ => null
            }
            : null;

    public static bool GetBool(this IReadOnlyDictionary<string, object> parameters, string key) =>
        parameters.TryGetValue(key, out var value)
        && value switch
        {
            bool flag => flag,
            string text => bool.TryParse(text, out var parsed) && parsed,
            _ => false
        };

    public static string? GetString(this IReadOnlyDictionary<string, object> parameters, string key) =>
        parameters.TryGetValue(key, out var value) ? value as string ?? value.ToString() : null;

    public static T? GetEnum<T>(this IReadOnlyDictionary<string, object> parameters, string key)
        where T : struct, Enum =>
        parameters.TryGetValue(key, out var value)
            ? value switch
            {
                T typed => typed,
                string text when Enum.TryParse<T>(text, true, out var parsed) => parsed,
                _ => null
            }
            : null;
}
