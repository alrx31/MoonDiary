using Microsoft.Data.Sqlite;

namespace MoonDiary.Data;

internal static class SqliteCommandExtensions
{
    public static SqliteCommand Command(
        this SqliteConnection connection,
        string sql,
        params (string Name, object? Value)[] parameters) =>
        connection.Command(null, sql, parameters);

    public static SqliteCommand Command(
        this SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql,
        params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var (name, value) in parameters)
        {
            command.Parameters.AddWithValue(name, SqlValues.DbNullable(value));
        }

        return command;
    }

    public static int Execute(
        this SqliteConnection connection,
        SqliteTransaction? transaction,
        string sql,
        params (string Name, object? Value)[] parameters)
    {
        using var command = connection.Command(transaction, sql, parameters);

        return command.ExecuteNonQuery();
    }

    public static int Execute(
        this SqliteConnection connection,
        string sql,
        params (string Name, object? Value)[] parameters) =>
        connection.Execute(null, sql, parameters);

    public static List<T> Query<T>(
        this SqliteConnection connection,
        Func<SqliteDataReader, T> map,
        string sql,
        params (string Name, object? Value)[] parameters)
    {
        using var command = connection.Command(sql, parameters);
        using var reader = command.ExecuteReader();
        var result = new List<T>();

        while (reader.Read())
        {
            result.Add(map(reader));
        }

        return result;
    }

    public static long ScalarLong(
        this SqliteConnection connection,
        string sql,
        params (string Name, object? Value)[] parameters)
    {
        using var command = connection.Command(sql, parameters);

        return Convert.ToInt64(command.ExecuteScalar() ?? 0L, System.Globalization.CultureInfo.InvariantCulture);
    }

    public static (string Sql, (string Name, object? Value)[] Parameters) InList(
        string prefix,
        IEnumerable<Guid> ids)
    {
        var parameters = ids.Select((id, index) => ($"${prefix}{index}", (object?)SqlValues.FromGuid(id))).ToArray();

        return (string.Join(", ", parameters.Select(p => p.Item1)), parameters);
    }
}
