namespace CardMicroserviceApi.Models;

/// <summary>
/// Flags enum so a single card always has exactly one value, while rules
/// (e.g. in <see cref="Services.CardActionsCalculator"/>) can combine
/// multiple statuses with bitwise OR.
/// </summary>
[Flags]
public enum CardStatus
{
    Ordered = 1 << 0,
    Inactive = 1 << 1,
    Active = 1 << 2,
    Restricted = 1 << 3,
    Blocked = 1 << 4,
    Expired = 1 << 5,
    Closed = 1 << 6
}
