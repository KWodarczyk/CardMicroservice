namespace CardMicroserviceApi.Models;

/// <summary>
/// Flags enum so a single card always has exactly one value, while rules
/// (e.g. in <see cref="Services.CardActionsCalculator"/>) can combine
/// multiple card types with bitwise OR.
/// </summary>
[Flags]
public enum CardType
{
    Prepaid = 1 << 0,
    Debit = 1 << 1,
    Credit = 1 << 2
}
