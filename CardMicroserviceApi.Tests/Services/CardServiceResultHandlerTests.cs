using CardMicroserviceApi.CardDataApiClient;
using CardMicroserviceApi.Models;
using CardMicroserviceApi.Services;
using Xunit;
using DtoCardType = CardMicroserviceApi.CardDataApiClient.CardType;
using DtoCardStatus = CardMicroserviceApi.CardDataApiClient.CardStatus;
using DomainCardType = CardMicroserviceApi.Models.CardType;
using DomainCardStatus = CardMicroserviceApi.Models.CardStatus;

namespace CardMicroserviceApi.Tests.Services;

public class CardServiceResultHandlerTests
{
    private static readonly CardInfoDto SampleCardInfo = new()
    {
        CardNumber = "Card11",
        CardType = DtoCardType.Debit,
        CardStatus = DtoCardStatus.Active,
        IsPinSet = true,
    };

    private static readonly CardDetails SampleCardDetails = new(
        CardNumber: "Card11",
        CardType: DomainCardType.Debit,
        CardStatus: DomainCardStatus.Active,
        IsPinSet: true);

    [Fact]
    public async Task Execute_ApiCallSucceeds_ReturnsSuccessWithMappedCardDetails()
    {
        var result = await CardServiceResultHandler.Execute(
            apiCall: () => Task.FromResult(SampleCardInfo),
            map: _ => SampleCardDetails);

        Assert.True(result.IsSuccess);
        Assert.Equal(SampleCardDetails, result.CardDetails);
    }

    [Fact]
    public async Task Execute_ApiExceptionWith404_ReturnsCardNotFound()
    {
        var result = await CardServiceResultHandler.Execute(
            apiCall: () => throw new ApiException("not found", 404, string.Empty, null, null),
            map: _ => SampleCardDetails);

        Assert.False(result.IsSuccess);
        Assert.Equal(CardServiceErrorCode.CardNotFound, result.ErrorCode);
    }

    [Fact]
    public async Task Execute_ApiExceptionWithOtherStatus_ReturnsUnexpectedError()
    {
        var result = await CardServiceResultHandler.Execute(
            apiCall: () => throw new ApiException("server error", 500, string.Empty, null, null),
            map: _ => SampleCardDetails);

        Assert.False(result.IsSuccess);
        Assert.Equal(CardServiceErrorCode.UnexpectedError, result.ErrorCode);
    }

    [Fact]
    public async Task Execute_HttpRequestException_ReturnsBackendUnavailable()
    {
        var result = await CardServiceResultHandler.Execute(
            apiCall: () => throw new HttpRequestException("connection refused"),
            map: _ => SampleCardDetails);

        Assert.False(result.IsSuccess);
        Assert.Equal(CardServiceErrorCode.BackendUnavailable, result.ErrorCode);
    }

    [Fact]
    public async Task Execute_TaskCanceledException_ReturnsBackendUnavailable()
    {
        var result = await CardServiceResultHandler.Execute(
            apiCall: () => throw new TaskCanceledException("timed out"),
            map: _ => SampleCardDetails);

        Assert.False(result.IsSuccess);
        Assert.Equal(CardServiceErrorCode.BackendUnavailable, result.ErrorCode);
    }
}
