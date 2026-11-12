using Microsoft.Data.Sqlite;

namespace MoonDiary.Data;

public sealed class SqliteConnectionFactory
{
    private readonly Func<string> pathProvider;

    public SqliteConnectionFactory(string databasePath)
        : this(() => databasePath)
    {
    }

    public SqliteConnectionFactory(Func<string> pathProvider)
    {
        this.pathProvider = pathProvider;
    }

    public string DatabasePath => pathProvider();

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
