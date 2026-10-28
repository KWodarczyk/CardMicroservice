using CardMicroserviceApi.CardDataApiClient;
using CardMicroserviceApi.Mappers;
using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

/// <summary>
/// Calls the CardDataApi backend microservice over HTTP using the NSwag-generated
/// client (CardDataServiceClient, regenerated from OpenApiSpecs/CardDataApi.swagger.json)
/// and maps the returned transport CardInfoDto into the internal CardDetails model.
/// </summary>
public class CardServiceClient : ICardService
{
    private readonly CardDataServiceClient _client;

    public CardServiceClient(CardDataServiceClient client)
    {
        _client = client;
    }

    public Task<CardDetailsResult> GetCardDetails(string userId, string cardNumber)
    {
        return CardServiceResultHandler.Execute(
            apiCall: () => _client.CardInfoAsync(userId, cardNumber),
            map: CardInfoMapper.MapToCardDetails);
    }
}
