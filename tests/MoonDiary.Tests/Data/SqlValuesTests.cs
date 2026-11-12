using MoonDiary.Core.Domain;
using MoonDiary.Data;

namespace MoonDiary.Tests.Data;

public class SqlValuesTests
{
    [Fact]
    public void Guid_RoundTrip_ReturnsSameValue()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id, SqlValues.ToGuid(SqlValues.FromGuid(id)));
    }

    [Fact]
    public void DateOnly_RoundTrip_UsesIsoFormat()
    {
        var date = new DateOnly(2026, 11, 9);

        Assert.Equal("2026-11-09", SqlValues.FromDateOnly(date));
        Assert.Equal(date, SqlValues.ToDateOnly("2026-11-09"));
    }

    [Fact]
    public void DateTime_RoundTrip_KeepsUtcKind()
    {
        var moment = new DateTime(2026, 11, 9, 17, 30, 15, DateTimeKind.Utc);

        var restored = SqlValues.ToUtcDateTime(SqlValues.FromUtcDateTime(moment));

        Assert.Equal(moment, restored);
        Assert.Equal(DateTimeKind.Utc, restored.Kind);
    }

    [Fact]
    public void DateTime_LocalKind_Throws()
    {
        var local = new DateTime(2026, 11, 9, 17, 30, 15, DateTimeKind.Local);

        Assert.Throws<ArgumentException>(() => SqlValues.FromUtcDateTime(local));
    }

    [Fact]
    public void TimeOnly_RoundTrip_UsesHoursAndMinutes()
    {
        Assert.Equal("21:15", SqlValues.FromTimeOnly(new TimeOnly(21, 15)));
        Assert.Equal(new TimeOnly(21, 15), SqlValues.ToTimeOnly("21:15"));
    }

    [Fact]
    public void Enum_RoundTrip_UsesCamelCase()
    {
        Assert.Equal("notRelevant", SqlValues.FromEnum(AdviceFeedback.NotRelevant));
        Assert.Equal(AdviceFeedback.NotRelevant, SqlValues.ToEnum<AdviceFeedback>("notRelevant"));
    }

    [Fact]
    public void StringList_RoundTrip_ReturnsSameItems()
    {
        var list = new List<string> { "кофе", "зал" };

        Assert.Equal(list, SqlValues.ToStringList(SqlValues.FromStringList(list)));
    }
}
