using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoonDiary.Core.Resources;

public static class ResourceStore
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static T ReadJson<T>(string fileName)
    {
        var assembly = typeof(ResourceStore).Assembly;
        var suffix = $".Resources.{fileName}";
        var resourceName = Array.Find(
            assembly.GetManifestResourceNames(),
            name => name.EndsWith(suffix, StringComparison.Ordinal));

        if (resourceName is null)
        {
            throw new InvalidOperationException($"Ресурс '{fileName}' не найден в сборке.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        var result = JsonSerializer.Deserialize<T>(stream, JsonOptions);

        if (result is null)
        {
            throw new InvalidOperationException($"Ресурс '{fileName}' пуст.");
        }

        return result;
    }
}
