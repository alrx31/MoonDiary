using MoonDiary.Core.Domain;

namespace MoonDiary.Core.Wizard;

public static class WizardSteps
{
    public static CardType? ToCardType(WizardStep step) => step switch
    {
        WizardStep.Done => CardType.Done,
        WizardStep.Achievement => CardType.Achievement,
        WizardStep.Feelings => CardType.Feelings,
        _ => null
    };

    public static WizardStep FromCardType(CardType cardType) => cardType switch
    {
        CardType.Done => WizardStep.Done,
        CardType.Achievement => WizardStep.Achievement,
        CardType.Feelings => WizardStep.Feelings,
        _ => throw new ArgumentOutOfRangeException(nameof(cardType), cardType, null)
    };
}
