using MoonDiary.Core.Insights;
using MoonDiary.Presentation.Formatting;

namespace MoonDiary.Presentation.ViewModels;

public sealed class AchievementRowViewModel
{
    public AchievementRowViewModel(MonthAchievement achievement)
    {
        Date = Ru.DayMonth(achievement.Date);
        Text = achievement.Text;
    }

    public string Date { get; }

    public string Text { get; }
}
