using System.Globalization;
using Microsoft.Data.Sqlite;
using MoonDiary.Data;

namespace MoonDiary.Tests.Support;

public sealed class TestDatabase : IDisposable
{
    private readonly string path;

    public TestDatabase(int? version = null)
    {
        path = Path.Combine(Path.GetTempPath(), $"moondiary-test-{Guid.NewGuid():N}.db");
        Factory = new SqliteConnectionFactory(path);

        if (version is null)
        {
            new DatabaseInitializer(Factory).Initialize();
        }
        else
        {
            using var connection = Factory.Open();
            MigrationRunner.MigrateTo(connection, version.Value);
        }
    }

    public SqliteConnectionFactory Factory { get; }

    public void Execute(string sql)
    {
        using var connection = Factory.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    public T Scalar<T>(string sql)
    {
        using var connection = Factory.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return (T)Convert.ChangeType(command.ExecuteScalar()!, typeof(T), CultureInfo.InvariantCulture);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            File.Delete(path + suffix);
        }
    }
}
