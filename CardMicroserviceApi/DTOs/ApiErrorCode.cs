namespace CardMicroserviceApi.DTOs;

/// <summary>
/// Machine-readable error codes returned by CardActionsController, covering both
/// request-validation failures and downstream card-service failures so API
/// consumers can branch on a stable code rather than parsing message text.
/// </summary>
public enum ApiErrorCode
{
    MissingUserId,
    MissingCardNumber,
    InvalidUserId,
    InvalidCardNumber,
    CardNotFound,
    BackendUnavailable,
    UnexpectedError,
}
