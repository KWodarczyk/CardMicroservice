using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

public interface ICardService
{
    /// <summary>
    /// Retrieves the card details for the given user and card number.
    /// Returns a failure result with a specific <see cref="CardServiceErrorCode"/>
    /// (e.g. CardNotFound) instead of null when the data cannot be retrieved.
    /// </summary>
    Task<CardDetailsResult> GetCardDetails(string userId, string cardNumber);
}
