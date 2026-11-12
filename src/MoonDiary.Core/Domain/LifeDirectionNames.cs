namespace MoonDiary.Core.Domain;

public static class LifeDirectionNames
{
    public static string Get(LifeDirection direction) => direction switch
    {
        LifeDirection.Sleep => "Сон",
        LifeDirection.Work => "Работа и учёба",
        LifeDirection.Health => "Здоровье и спорт",
        LifeDirection.Relations => "Отношения",
        LifeDirection.Household => "Быт",
        LifeDirection.Emotions => "Эмоции",
        LifeDirection.None => "Без направления",
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
    };
}
