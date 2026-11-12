using Microsoft.Data.Sqlite;
using MoonDiary.Data;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Data;

public class MigrationRunnerTests
{
    [Fact]
    public void Initialize_NewDatabase_HasLatestVersionAndEightTables()
    {
        using var database = new TestDatabase();

        Assert.Equal(MigrationRunner.LatestVersion, database.Scalar<int>("PRAGMA user_version"));
        Assert.Equal(8, database.Scalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%'"));
    }

    [Fact]
    public void Initialize_Twice_KeepsVersion()
    {
        using var database = new TestDatabase();

        new DatabaseInitializer(database.Factory).Initialize();

        Assert.Equal(MigrationRunner.LatestVersion, database.Scalar<int>("PRAGMA user_version"));
    }

    [Fact]
    public void Migrate_NewerDatabase_Throws()
    {
        using var database = new TestDatabase();
        database.Execute("PRAGMA user_version = 999");

        using var connection = database.Factory.Open();

        Assert.Throws<InvalidOperationException>(() => MigrationRunner.Migrate(connection));
    }

    [Fact]
    public void Migrate_FromVersionOneWithData_KeepsData()
    {
        using var database = new TestDatabase(1);
        database.Execute(
            "INSERT INTO entries (id, type, day_key, energy, positivity, created_at_utc, updated_at_utc, " +
            "timezone_offset, is_backfilled) VALUES ('11111111-1111-1111-1111-111111111111', 'evening', " +
            "'2026-11-09', 4, 3, '2026-11-09T17:00:00.0000000Z', '2026-11-09T17:00:00.0000000Z', 180, 0)");

        using (var connection = database.Factory.Open())
        {
            MigrationRunner.Migrate(connection);
        }

        Assert.Equal(1, database.Scalar<int>("SELECT COUNT(*) FROM entries"));
        Assert.Equal(MigrationRunner.LatestVersion, database.Scalar<int>("PRAGMA user_version"));
    }

    [Fact]
    public void Schema_EnergyOutOfRange_IsRejected()
    {
        using var database = new TestDatabase();

        Assert.Throws<SqliteException>(() => database.Execute(Insert("a", "evening", "2026-11-09", 6)));
    }

    [Fact]
    public void Schema_TwoEveningsPerDay_IsRejectedButCheckinsAllowed()
    {
        using var database = new TestDatabase();
        database.Execute(Insert("a", "evening", "2026-11-09", 3));
        database.Execute(Insert("b", "checkin", "2026-11-09", 3));
        database.Execute(Insert("c", "checkin", "2026-11-09", 3));

        Assert.Throws<SqliteException>(() => database.Execute(Insert("d", "evening", "2026-11-09", 3)));
    }

    [Fact]
    public void Schema_SecondSettingsRow_IsRejected()
    {
        using var database = new TestDatabase();

        Assert.Throws<SqliteException>(() => database.Execute(
            "INSERT INTO settings (id, reminder_enabled, reminder_time, adaptive_reminder, theme, onboarding_done) " +
            "VALUES (2, 1, '18:00', 1, 'dark', 0)"));
    }

    private static string Insert(string id, string type, string day, int energy) =>
        "INSERT INTO entries (id, type, day_key, energy, positivity, created_at_utc, updated_at_utc, " +
        $"timezone_offset, is_backfilled) VALUES ('{id}', '{type}', '{day}', {energy}, 3, " +
        "'2026-11-09T17:00:00.0000000Z', '2026-11-09T17:00:00.0000000Z', 180, 0)";
}
