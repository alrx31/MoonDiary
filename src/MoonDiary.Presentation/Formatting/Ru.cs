using System.Globalization;

namespace MoonDiary.Presentation.Formatting;

public static class Ru
{
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("ru-RU");

    private static readonly string[] MonthNames =
    [
        "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
    ];

    private static readonly string[] MonthGenitive =
    [
        "января", "февраля", "марта", "апреля", "мая", "июня",
        "июля", "августа", "сентября", "октября", "ноября", "декабря"
    ];

    private static readonly string[] WeekdayNames =
        ["Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье"];

    private static readonly string[] WeekdayShort = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"];

    public static IReadOnlyList<string> WeekdayHeaders => WeekdayShort;

    public static string DayTitle(DateOnly date) =>
        $"{WeekdayNames[WeekdayIndex(date)]}, {date.Day} {MonthGenitive[date.Month - 1]}";

    public static string DayMonth(DateOnly date) => $"{date.Day} {MonthGenitive[date.Month - 1]}";

    public static string FullDate(DateOnly date) => $"{date.Day} {MonthGenitive[date.Month - 1]} {date.Year}";

    public static string MonthTitle(int year, int month) => $"{MonthNames[month - 1]} {year}";

    public static string FeedDate(DateOnly date) => $"{WeekdayShort[WeekdayIndex(date)]} {date.Day}";

    public static string WeekRange(DateOnly start, DateOnly end) =>
        start.Month == end.Month
            ? $"{start.Day}–{end.Day} {MonthGenitive[end.Month - 1]}"
            : $"{DayMonth(start)} – {DayMonth(end)}";

    public static string Time(DateTime utc, int offsetMinutes) =>
        utc.AddMinutes(offsetMinutes).ToString("HH:mm", CultureInfo.InvariantCulture);

    public static string Time(TimeSpan time) =>
        new TimeOnly(time.Ticks % TimeSpan.TicksPerDay).ToString("HH:mm", CultureInfo.InvariantCulture);

    public static string Number(double value) =>
        Math.Round(value, 1, MidpointRounding.AwayFromZero).ToString("0.0", Culture);

    public static string WithSign(double value)
    {
        var rounded = Math.Round(value, 1, MidpointRounding.AwayFromZero);

        return (rounded < 0 ? "−" : "+") + Math.Abs(rounded).ToString("0.0", Culture);
    }

    public static string Plural(int count, string one, string few, string many)
    {
        var tens = Math.Abs(count) % 100;
        var units = tens % 10;

        if (tens is >= 11 and <= 14)
        {
            return many;
        }

        return units switch
        {
            1 => one,
            >= 2 and <= 4 => few,
            _ => many
        };
    }

    public static string Count(int count, string one, string few, string many) =>
        $"{count.ToString(CultureInfo.InvariantCulture)} {Plural(count, one, few, many)}";

    private static int WeekdayIndex(DateOnly date) => ((int)date.DayOfWeek + 6) % 7;
}
