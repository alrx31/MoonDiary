using System.Text.Json;
using MoonDiary.Core.Domain;
using MoonDiary.Core.Resources;

namespace MoonDiary.Tests.Core.Resources;

public class ResourceStoreTests
{
    [Fact]
    public void ReadJson_UnknownFile_ThrowsInvalidOperation()
    {
        var error = Assert.Throws<InvalidOperationException>(
            () => ResourceStore.ReadJson<object>("missing.json"));

        Assert.Contains("missing.json", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void JsonOptions_EnumValue_ReadsCamelCase()
    {
        var json = """
            { "status": "insufficientData" }
            """;

        var result = JsonSerializer.Deserialize<EnumHolder>(json, ResourceStore.JsonOptions);

        Assert.NotNull(result);
        Assert.Equal(ReportStatus.InsufficientData, result.Status);
    }

    private sealed class EnumHolder
    {
        public ReportStatus Status { get; set; }
    }
}
