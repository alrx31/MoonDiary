using System.Text.RegularExpressions;
using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Tags;

public static partial class TagTextComposer
{
    public static bool Contains(string? text, string label) =>
        !string.IsNullOrEmpty(text) && text.Contains(label, StringComparison.Ordinal);

    public static string Append(string? text, string label)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return label;
        }

        var trimmed = text.TrimEnd();
        var result = trimmed + (trimmed.EndsWith(',') ? " " : ", ") + label;

        return result.Length > TextLimits.CardText ? text : result;
    }

    public static string Remove(string text, string label)
    {
        var index = text.IndexOf(label, StringComparison.Ordinal);

        if (index < 0)
        {
            return text;
        }

        var result = text.Remove(index, label.Length);
        result = RepeatedCommas().Replace(result, ", ");

        return result.Trim(' ', ',');
    }

    public static string Toggle(string? text, string label) =>
        Contains(text, label) ? Remove(text!, label) : Append(text, label);

    public static IReadOnlyList<Guid> ResolveLinkedTagIds(string? text, IEnumerable<Tag> selected)
    {
        var result = new List<Guid>();

        foreach (var tag in selected)
        {
            if (Contains(text, tag.Label) && !result.Contains(tag.Id))
            {
                result.Add(tag.Id);
            }
        }

        return result;
    }

    [GeneratedRegex(@"[ ]*,[ ,]*")]
    private static partial Regex RepeatedCommas();
}
