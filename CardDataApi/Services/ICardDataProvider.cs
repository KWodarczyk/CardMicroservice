using CardDataApi.Models;

namespace CardDataApi.Services;

public interface ICardDataProvider
{
    /// <summary>
    /// Retrieves the card data for the given user and card number.
    /// Returns null if the user or the card does not exist.
    /// </summary>
    CardRecord? GetCard(string userId, string cardNumber);
}

/// <summary>Internal representation of a stored card (kept separate from the wire DTO).</summary>
public record CardRecord(string CardNumber, CardType CardType, CardStatus CardStatus, bool IsPinSet);
