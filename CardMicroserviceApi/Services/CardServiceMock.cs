using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

/// <summary>
/// Mock in-memory implementation of ICardService, used in place of CardServiceClient
/// when "CardDataApi:UseMock" is set to true in configuration. Useful for local
/// development/demo purposes without needing the CardDataApi backend running.
/// </summary>
public class CardServiceMock : ICardService
{
    private readonly Dictionary<string, Dictionary<string, CardDetails>> _userCards = CreateSampleUserCards();

    public Task<CardDetailsResult> GetCardDetails(string userId, string cardNumber)
    {
        if (!_userCards.TryGetValue(userId, out var cards)
            || !cards.TryGetValue(cardNumber, out var cardDetails))
        {
            return Task.FromResult(CardDetailsResult.Failure(CardServiceErrorCode.CardNotFound));
        }

        return Task.FromResult(CardDetailsResult.Success(cardDetails));
    }

    private static Dictionary<string, Dictionary<string, CardDetails>> CreateSampleUserCards()
    {
        var userCards = new Dictionary<string, Dictionary<string, CardDetails>>();
        for (var i = 1; i <= 3; i++)
        {
            var cards = new Dictionary<string, CardDetails>();
            var cardIndex = 1;
            foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
            {
                foreach (CardStatus cardStatus in Enum.GetValues(typeof(CardStatus)))
                {
                    var cardNumber = $"Card{i}{cardIndex}";
                    cards.Add(cardNumber,
                        new CardDetails(
                            CardNumber: cardNumber,
                            CardType: cardType,
                            CardStatus: cardStatus,
                            IsPinSet: cardIndex % 2 == 0));
                    cardIndex++;
                }
            }

            var userId = $"User{i}";
            userCards.Add(userId, cards);
        }

        return userCards;
    }
}
