using CardMicroserviceApi.CardDataApiClient;
using CardMicroserviceApi.Mappers;
using Xunit;
using DtoCardType = CardMicroserviceApi.CardDataApiClient.CardType;
using DtoCardStatus = CardMicroserviceApi.CardDataApiClient.CardStatus;
using DomainCardType = CardMicroserviceApi.Models.CardType;
using DomainCardStatus = CardMicroserviceApi.Models.CardStatus;

namespace CardMicroserviceApi.Tests.Mappers;

public class CardInfoMapperTests
{
    [Theory]
    [InlineData(DtoCardType.Prepaid, DomainCardType.Prepaid)]
    [InlineData(DtoCardType.Debit, DomainCardType.Debit)]
    [InlineData(DtoCardType.Credit, DomainCardType.Credit)]
    public void MapToCardDetails_MapsCardTypeByName(DtoCardType generatedCardType, DomainCardType expectedCardType)
    {
        var cardInfo = new CardInfoDto
        {
            CardNumber = "Card11",
            CardType = generatedCardType,
            CardStatus = DtoCardStatus.Active,
            IsPinSet = true,
        };

        var result = CardInfoMapper.MapToCardDetails(cardInfo);

        Assert.Equal(expectedCardType, result.CardType);
    }

    [Theory]
    [InlineData(DtoCardStatus.Ordered, DomainCardStatus.Ordered)]
    [InlineData(DtoCardStatus.Inactive, DomainCardStatus.Inactive)]
    [InlineData(DtoCardStatus.Active, DomainCardStatus.Active)]
    [InlineData(DtoCardStatus.Restricted, DomainCardStatus.Restricted)]
    [InlineData(DtoCardStatus.Blocked, DomainCardStatus.Blocked)]
    [InlineData(DtoCardStatus.Expired, DomainCardStatus.Expired)]
    [InlineData(DtoCardStatus.Closed, DomainCardStatus.Closed)]
    public void MapToCardDetails_MapsCardStatusByName(DtoCardStatus generatedCardStatus, DomainCardStatus expectedCardStatus)
    {
        var cardInfo = new CardInfoDto
        {
            CardNumber = "Card11",
            CardType = DtoCardType.Credit,
            CardStatus = generatedCardStatus,
            IsPinSet = false,
        };

        var result = CardInfoMapper.MapToCardDetails(cardInfo);

        Assert.Equal(expectedCardStatus, result.CardStatus);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MapToCardDetails_MapsCardNumberAndIsPinSet(bool isPinSet)
    {
        var cardInfo = new CardInfoDto
        {
            CardNumber = "Card42",
            CardType = DtoCardType.Debit,
            CardStatus = DtoCardStatus.Active,
            IsPinSet = isPinSet,
        };

        var result = CardInfoMapper.MapToCardDetails(cardInfo);

        Assert.Equal("Card42", result.CardNumber);
        Assert.Equal(isPinSet, result.IsPinSet);
    }
}
