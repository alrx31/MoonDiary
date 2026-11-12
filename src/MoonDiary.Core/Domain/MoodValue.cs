namespace MoonDiary.Core.Domain;

public static class MoodValue
{
    public const int Min = 1;

    public const int Max = 5;

    public static bool IsValid(int value) => value is >= Min and <= Max;

    public static int EnsureValid(int value, string paramName)
    {
        if (!IsValid(value))
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"Значение шкалы должно быть от {Min} до {Max}.");
        }

        return value;
    }
}
