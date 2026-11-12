using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using MoonDiary.Core.Abstractions;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Time;

namespace MoonDiary.Core.Transfer;

public sealed class DataTransferService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly IDataTransferRepository repository;
    private readonly DayClock clock;
    private readonly int currentSchemaVersion;

    public DataTransferService(IDataTransferRepository repository, DayClock clock, int currentSchemaVersion)
    {
        this.repository = repository;
        this.clock = clock;
        this.currentSchemaVersion = currentSchemaVersion;
    }

    public async Task<string> ExportJsonAsync()
    {
        var bundle = await repository.ReadAllAsync();
        bundle.SchemaVersion = currentSchemaVersion;
        bundle.ExportedAtUtc = clock.UtcNow;

        return JsonSerializer.Serialize(bundle, JsonOptions);
    }

    public string SuggestFileName() =>
        $"moondiary-export-{clock.LocalNow.ToString("yyyy-MM-dd-HHmm", CultureInfo.InvariantCulture)}.json";

    public ExportBundle Parse(string json)
    {
        ExportBundle? bundle;

        try
        {
            bundle = JsonSerializer.Deserialize<ExportBundle>(json, JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new ImportException("Файл не похож на экспорт MoonDiary", exception);
        }

        if (bundle is null || bundle.SchemaVersion <= 0 || bundle.Entries is null || bundle.Tags is null)
        {
            throw new ImportException("Файл не похож на экспорт MoonDiary");
        }

        if (bundle.SchemaVersion > currentSchemaVersion)
        {
            throw new ImportException("Файл создан более новой версией приложения");
        }

        bundle.EntryTags ??= [];
        bundle.Reports ??= [];
        bundle.Advice ??= [];
        bundle.Drafts ??= [];
        bundle.Letters ??= [];
        bundle.Settings ??= new AppSettings();

        if (bundle.Entries.Any(entry => !MoodValue.IsValid(entry.Energy) || !MoodValue.IsValid(entry.Positivity)))
        {
            throw new ImportException("В файле есть запись со значением шкалы вне диапазона 1–5");
        }

        var duplicateEvening = bundle.Entries
            .Where(entry => entry.Type == EntryType.Evening)
            .GroupBy(entry => entry.DayKey)
            .Any(group => group.Count() > 1);

        if (duplicateEvening)
        {
            throw new ImportException("В файле две вечерние записи за один день");
        }

        return bundle;
    }

    public async Task ImportJsonAsync(string json, Func<string, string, Task> saveBackup)
    {
        var bundle = Parse(json);

        await saveBackup(SuggestFileName(), await ExportJsonAsync());

        try
        {
            await repository.ReplaceAllAsync(bundle);
        }
        catch (Exception exception) when (exception is not ImportException)
        {
            throw new ImportException("Не удалось загрузить данные из файла, текущие данные сохранены", exception);
        }
    }
}
