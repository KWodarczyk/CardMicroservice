using CardMicroserviceApi.CardDataApiClient;
using CardMicroserviceApi.Models;
using DomainCardType = CardMicroserviceApi.Models.CardType;
using DomainCardStatus = CardMicroserviceApi.Models.CardStatus;

namespace CardMicroserviceApi.Mappers;

/// <summary>
/// Maps the NSwag-generated transport <see cref="CardInfoDto"/> (from CardDataApi)
/// into the internal <see cref="CardDetails"/> domain model. Extracted from
/// CardServiceClient so it can be unit tested directly without an HTTP client.
/// </summary>
public static class CardInfoMapper
{
    public static CardDetails MapToCardDetails(CardInfoDto cardInfo)
    {
        // Generated enum values are not [Flags]-compatible with the internal bit-based
        // enums, so values must be matched by name rather than cast numerically.
        var cardType = Enum.Parse<DomainCardType>(cardInfo.CardType.ToString());
        var cardStatus = Enum.Parse<DomainCardStatus>(cardInfo.CardStatus.ToString());

        return new CardDetails(
            CardNumber: cardInfo.CardNumber,
            CardType: cardType,
            CardStatus: cardStatus,
            IsPinSet: cardInfo.IsPinSet);
    }
}
