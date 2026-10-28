using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

/// <summary>
/// Defines, for a single action, which card types it applies to and, for each
/// allowed card status, which PIN condition applies. Keying the PIN condition
/// directly by status makes it impossible to see a status without its condition.
/// </summary>
internal sealed class ActionRule
{
    public required CardAction ActionName { get; init; }
    public required CardType AllowedCardTypes { get; init; }
    public required IReadOnlyDictionary<CardStatus, PinCondition> AllowedStatuses { get; init; }
}
