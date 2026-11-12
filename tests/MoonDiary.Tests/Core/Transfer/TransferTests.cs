using MoonDiary.Core.Domain;
using MoonDiary.Core.Transfer;
using MoonDiary.Data;
using MoonDiary.Tests.Support;

namespace MoonDiary.Tests.Core.Transfer;

public class TransferTests
{
    [Fact]
    public async Task Export_FilledDatabase_ContainsAllSections()
    {
        using var app = await FilledAsync();

        var json = await app.TransferService.ExportJsonAsync();
        var bundle = app.TransferService.Parse(json);

        Assert.Equal(MigrationRunner.LatestVersion, bundle.SchemaVersion);
        Assert.Equal(app.Clock.UtcNow, bundle.ExportedAtUtc);
        Assert.Equal(2, bundle.Entries.Count);
        Assert.Equal(30, bundle.Tags.Count);
        Assert.Single(bundle.EntryTags);
        Assert.Single(bundle.Reports);
        Assert.Single(bundle.Drafts);
        Assert.Single(bundle.Letters);
        Assert.Equal("Алексей", bundle.Settings.UserName);
        Assert.Contains("\"type\": \"evening\"", json);
        Assert.Equal("moondiary-export-2026-11-16-0900.json", app.TransferService.SuggestFileName());
    }

    [Fact]
    public async Task Import_IntoEmptyDatabase_RestoresSameData()
    {
        using var source = await FilledAsync();
        using var target = TestApp.At(2026, 11, 16, 9, 0);
        var json = await source.TransferService.ExportJsonAsync();

        await target.TransferService.ImportJsonAsync(json, (_, _) => Task.CompletedTask);

        Assert.Equal(json, await target.TransferService.ExportJsonAsync());
    }

    [Fact]
    public async Task Import_ReplacesExistingDataAfterBackup()
    {
        using var source = TestApp.At(2026, 11, 16, 9, 0);
        using var target = await FilledAsync();
        var json = await source.TransferService.ExportJsonAsync();
        string? backup = null;

        await target.TransferService.ImportJsonAsync(json, (name, content) =>
        {
            backup = content;
            Assert.EndsWith(".json", name);

            return Task.CompletedTask;
        });

        Assert.Contains("Алексей", backup);
        Assert.Equal(0, target.Database.Scalar<int>("SELECT COUNT(*) FROM entries"));
        Assert.Equal(0, target.Database.Scalar<int>("SELECT COUNT(*) FROM tags"));
    }

    [Theory]
    [InlineData("{oops")]
    [InlineData("{\"schemaVersion\":999,\"entries\":[],\"tags\":[]}")]
    [InlineData("{\"schemaVersion\":1,\"tags\":[],\"entries\":[{\"id\":\"11111111-1111-1111-1111-111111111111\"," +
                "\"type\":\"evening\",\"dayKey\":\"2026-11-09\",\"energy\":7,\"positivity\":3," +
                "\"createdAtUtc\":\"2026-11-09T17:00:00Z\",\"updatedAtUtc\":\"2026-11-09T17:00:00Z\"}]}")]
    public async Task Import_InvalidFile_ThrowsAndKeepsData(string json)
    {
        using var app = await FilledAsync();
        var backupCalled = false;

        await Assert.ThrowsAsync<ImportException>(() => app.TransferService.ImportJsonAsync(json, (_, _) =>
        {
            backupCalled = true;

            return Task.CompletedTask;
        }));

        Assert.False(backupCalled);
        Assert.Equal(2, app.Database.Scalar<int>("SELECT COUNT(*) FROM entries"));
    }

    [Fact]
    public async Task Import_BackupFails_StopsImport()
    {
        using var source = TestApp.At(2026, 11, 16, 9, 0);
        using var target = await FilledAsync();
        var json = await source.TransferService.ExportJsonAsync();

        await Assert.ThrowsAsync<IOException>(
            () => target.TransferService.ImportJsonAsync(json, (_, _) => throw new IOException("диск заполнен")));

        Assert.Equal(2, target.Database.Scalar<int>("SELECT COUNT(*) FROM entries"));
    }

    private static async Task<TestApp> FilledAsync()
    {
        var app = await TestApp.At(2026, 11, 16, 9, 0).SeedTagsAsync();
        await app.EveningAsync(Dates.D(2026, 11, 15), 4, 3, (CardType.Done, "спорт"));
        await app.CheckinService.CreateAsync(3, 3, "день");
        await app.ReportService.EnsureLastWeekReportAsync();
        await app.LetterService.CreateAsync("привет", Dates.D(2026, 12, 1));

        var draft = await app.EntryService.OpenWizardAsync(Dates.D(2026, 11, 14));
        draft.SetEnergy(2);
        await app.EntryService.SaveDraftAsync(draft);

        var settings = await app.Settings.GetAsync();
        settings.UserName = "Алексей";
        await app.Settings.SaveAsync(settings);

        return app;
    }
}
