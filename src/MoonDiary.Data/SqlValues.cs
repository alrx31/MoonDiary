using System.Globalization;
using System.Text.Json;

namespace MoonDiary.Data;

public static class SqlValues
{
    public static string FromGuid(Guid value) => value.ToString("D");

    public static Guid ToGuid(string value) => Guid.ParseExact(value, "D");

    public static string FromDateOnly(DateOnly value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static DateOnly ToDateOnly(string value) =>
        DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string FromUtcDateTime(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Момент времени должен быть в UTC.", nameof(value));
        }

        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public static DateTime ToUtcDateTime(string value) =>
        DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();

    public static string FromTimeOnly(TimeOnly value) => value.ToString("HH:mm", CultureInfo.InvariantCulture);

    public static TimeOnly ToTimeOnly(string value) =>
        TimeOnly.ParseExact(value, "HH:mm", CultureInfo.InvariantCulture);

    public static string FromEnum<T>(T value)
        where T : struct, Enum
    {
        var name = value.ToString();

        return char.ToLowerInvariant(name[0]) + name[1..];
    }

    public static T ToEnum<T>(string value)
        where T : struct, Enum =>
        Enum.Parse<T>(value, ignoreCase: true);

    public static string FromStringList(IEnumerable<string> value) => JsonSerializer.Serialize(value);

    public static List<string> ToStringList(string value) =>
        JsonSerializer.Deserialize<List<string>>(value) ?? [];

    public static object DbNullable(object? value) => value ?? DBNull.Value;
}
