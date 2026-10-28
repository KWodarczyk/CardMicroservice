using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

/// <summary>
/// Result of an <see cref="ICardService.GetCardDetails"/> call. Exactly one of
/// <see cref="CardDetails"/> or <see cref="ErrorCode"/> is set, avoiding the need
/// for callers to treat a null return value as "not found".
/// </summary>
public readonly struct CardDetailsResult
{
    public CardDetails? CardDetails { get; }
    public CardServiceErrorCode? ErrorCode { get; }

    public bool IsSuccess => ErrorCode is null;

    private CardDetailsResult(CardDetails? cardDetails, CardServiceErrorCode? errorCode)
    {
        CardDetails = cardDetails;
        ErrorCode = errorCode;
    }

    public static CardDetailsResult Success(CardDetails cardDetails) => new(cardDetails, errorCode: null);

    public static CardDetailsResult Failure(CardServiceErrorCode errorCode) => new(cardDetails: null, errorCode);
}
