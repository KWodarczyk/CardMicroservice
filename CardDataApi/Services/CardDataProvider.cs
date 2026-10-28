using CardDataApi.Models;

namespace CardDataApi.Services;

/// <summary>
/// Sample in-memory card data provider. In a real system this would query a
/// database or another backing store instead of returning generated sample data.
/// This is the same sample data that CardMicroserviceApi previously generated
/// in-process via CardServiceMock, now moved to this backend microservice.
/// </summary>
public class CardDataProvider : ICardDataProvider
{
    private readonly Dictionary<string, Dictionary<string, CardRecord>> _userCards = CreateSampleUserCards();

    public CardRecord? GetCard(string userId, string cardNumber)
    {
        if (!_userCards.TryGetValue(userId, out Dictionary<string, CardRecord>? cards)
            || !cards.TryGetValue(cardNumber, out CardRecord? card))
        {
            return null;
        }

        return card;
    }

    private static Dictionary<string, Dictionary<string, CardRecord>> CreateSampleUserCards()
    {
        var userCards = new Dictionary<string, Dictionary<string, CardRecord>>();
        for (var i = 1; i <= 3; i++)
        {
            var cards = new Dictionary<string, CardRecord>();
            var cardIndex = 1;
            foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
            {
                foreach (CardStatus cardStatus in Enum.GetValues(typeof(CardStatus)))
                {
                    var cardNumber = $"Card{i}{cardIndex}";
                    cards.Add(cardNumber,
                        new CardRecord(
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
