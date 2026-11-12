using System.Globalization;
using Microsoft.Data.Sqlite;
using MoonDiary.Core.Tags;
using MoonDiary.Data;
using MoonDiary.Presentation.Services;

namespace MoonDiary.Services;

public sealed class AppBootstrapper : IAppBootstrapper
{
    private static readonly string[] JournalSuffixes = ["-wal", "-shm"];

    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly SqliteConnectionFactory factory;
    private readonly TagService tags;
    private readonly TimeProvider timeProvider;
    private readonly IServiceProvider services;

    public AppBootstrapper(
        SqliteConnectionFactory factory,
        TagService tags,
        TimeProvider timeProvider,
        IServiceProvider services)
    {
        this.factory = factory;
        this.tags = tags;
        this.timeProvider = timeProvider;
        this.services = services;
    }

    public async Task InitializeAsync()
    {
        await gate.WaitAsync();

        try
        {
            await Task.Run(() => new DatabaseInitializer(factory).Initialize());
            await tags.SeedStarterTagsAsync();
#if DEBUG
            if (LaunchOptions.Current.Seed is { } seed)
            {
                await services.GetRequiredService<DebugSeeder>().SeedAsync(seed);
            }
#endif
        }
        finally
        {
            gate.Release();
        }
    }

    public Task<string?> QuarantineDatabaseAsync() => Task.Run(() =>
    {
        SqliteConnection.ClearAllPools();

        if (!File.Exists(factory.DatabasePath))
        {
            return null;
        }

        var stamp = timeProvider.GetUtcNow().ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var target = $"{factory.DatabasePath}.broken-{stamp}";

        File.Move(factory.DatabasePath, target);

        foreach (var suffix in JournalSuffixes)
        {
            if (File.Exists(factory.DatabasePath + suffix))
            {
                File.Move(factory.DatabasePath + suffix, target + suffix);
            }
        }

        return (string?)target;
    });
}
