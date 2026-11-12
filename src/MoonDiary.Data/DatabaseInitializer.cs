namespace MoonDiary.Data;

public sealed class DatabaseInitializer
{
    private readonly SqliteConnectionFactory factory;

    public DatabaseInitializer(SqliteConnectionFactory factory)
    {
        this.factory = factory;
    }

    public void Initialize()
    {
        var directory = Path.GetDirectoryName(factory.DatabasePath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = factory.Open();
        MigrationRunner.Migrate(connection);
    }
}
