namespace MoonDiary.Core.Domain;

public static class MoodLabels
{
    private static readonly string[] EnergyLabels = ["истощение", "усталость", "ровно", "бодрость", "подъём"];

    private static readonly string[] PositivityLabels = ["плохо", "грустно", "нейтрально", "хорошо", "отлично"];

    public static string Get(MoodScale scale, int value)
    {
        var labels = scale switch
        {
            MoodScale.Energy => EnergyLabels,
            MoodScale.Positivity => PositivityLabels,
            _ => throw new ArgumentOutOfRangeException(nameof(scale), scale, null)
        };

        return labels[MoodValue.EnsureValid(value, nameof(value)) - MoodValue.Min];
    }
}
