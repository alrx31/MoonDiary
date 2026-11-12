using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MoonDiary.Data;

public static class MigrationRunner
{
    private static readonly Lazy<List<(int Version, string Sql)>> Scripts = new(LoadScripts);

    public static int LatestVersion => Scripts.Value.Count == 0 ? 0 : Scripts.Value[^1].Version;

    public static int GetVersion(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA user_version;";

        return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
    }

    public static void Migrate(SqliteConnection connection) => MigrateTo(connection, LatestVersion);

    public static void MigrateTo(SqliteConnection connection, int version)
    {
        var current = GetVersion(connection);

        if (current > LatestVersion)
        {
            throw new InvalidOperationException("база создана более новой версией приложения");
        }

        foreach (var script in Scripts.Value)
        {
            if (script.Version <= current || script.Version > version)
            {
                continue;
            }

            using var transaction = connection.BeginTransaction();
            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = script.Sql;
                command.ExecuteNonQuery();
            }

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $"PRAGMA user_version = {script.Version.ToString(CultureInfo.InvariantCulture)};";
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    private static List<(int Version, string Sql)> LoadScripts()
    {
        var assembly = typeof(MigrationRunner).Assembly;
        var result = new List<(int Version, string Sql)>();

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (!name.EndsWith(".sql", StringComparison.Ordinal))
            {
                continue;
            }

            var parts = name.Split('.');
            var file = parts[^2];
            var version = int.Parse(file[..3], CultureInfo.InvariantCulture);

            using var stream = assembly.GetManifestResourceStream(name)!;
            using var reader = new StreamReader(stream);
            result.Add((version, reader.ReadToEnd()));
        }

        return result.OrderBy(script => script.Version).ToList();
    }
}
