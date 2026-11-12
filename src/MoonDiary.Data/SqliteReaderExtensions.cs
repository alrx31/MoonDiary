using Microsoft.Data.Sqlite;

namespace MoonDiary.Data;

public static class SqliteReaderExtensions
{
    public static string GetText(this SqliteDataReader reader, string column) =>
        reader.GetString(reader.GetOrdinal(column));

    public static string? GetNullableString(this SqliteDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);

        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static int GetInt(this SqliteDataReader reader, string column) =>
        reader.GetInt32(reader.GetOrdinal(column));

    public static bool GetBool(this SqliteDataReader reader, string column) =>
        reader.GetInt64(reader.GetOrdinal(column)) != 0;

    public static Guid GetGuid(this SqliteDataReader reader, string column) =>
        SqlValues.ToGuid(reader.GetText(column));

    public static DateOnly GetDateOnly(this SqliteDataReader reader, string column) =>
        SqlValues.ToDateOnly(reader.GetText(column));

    public static TimeOnly GetTimeOnly(this SqliteDataReader reader, string column) =>
        SqlValues.ToTimeOnly(reader.GetText(column));

    public static DateTime GetUtcDateTime(this SqliteDataReader reader, string column) =>
        SqlValues.ToUtcDateTime(reader.GetText(column));

    public static DateTime? GetNullableUtcDateTime(this SqliteDataReader reader, string column)
    {
        var value = reader.GetNullableString(column);

        return value is null ? null : SqlValues.ToUtcDateTime(value);
    }

    public static T GetEnum<T>(this SqliteDataReader reader, string column)
        where T : struct, Enum =>
        SqlValues.ToEnum<T>(reader.GetText(column));

    public static T? GetNullableEnum<T>(this SqliteDataReader reader, string column)
        where T : struct, Enum
    {
        var value = reader.GetNullableString(column);

        return value is null ? null : SqlValues.ToEnum<T>(value);
    }
}
