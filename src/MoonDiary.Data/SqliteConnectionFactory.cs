using Microsoft.Data.Sqlite;

namespace MoonDiary.Data;

public sealed class SqliteConnectionFactory
{
    public SqliteConnectionFactory(string databasePath)
    {
        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public SqliteConnection Open()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = DatabasePath,
            ForeignKeys = true
        };
        var connection = new SqliteConnection(builder.ToString());
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();

        return connection;
    }
}
