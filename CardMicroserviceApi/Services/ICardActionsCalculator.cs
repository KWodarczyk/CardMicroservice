using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

public interface ICardActionsCalculator
{
    /// <summary>
    /// Returns the list of actions allowed for a card with the given type, status and PIN state.
    /// </summary>
    IReadOnlyList<CardAction> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet);
}
