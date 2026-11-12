using System.Globalization;
using System.Text.RegularExpressions;

namespace MoonDiary.Core.Analytics;

public static partial class TemplateRenderer
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

    public static string Render(string template, IReadOnlyDictionary<string, string> values)
    {
        var result = Placeholder().Replace(
            template,
            match => values.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);

        if (Placeholder().IsMatch(StripValues(template, values)))
        {
            throw new InvalidOperationException($"В шаблоне остался незаменённый плейсхолдер: {template}");
        }

        return result;
    }

    public static string Render(string template, params (string Key, string Value)[] values) =>
        Render(template, values.ToDictionary(pair => pair.Key, pair => pair.Value));

    public static string Number(double value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0", Russian);

    private static string StripValues(string template, IReadOnlyDictionary<string, string> values) =>
        Placeholder().Replace(
            template,
            match => values.ContainsKey(match.Groups[1].Value) ? string.Empty : match.Value);

    [GeneratedRegex(@"\{([a-z]+)\}")]
    private static partial Regex Placeholder();
}
