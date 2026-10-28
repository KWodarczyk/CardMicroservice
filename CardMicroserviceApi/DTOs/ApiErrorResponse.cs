namespace CardMicroserviceApi.DTOs;

/// <summary>
/// Error payload returned for any non-2xx response from CardActionsController.
/// </summary>
public record ApiErrorResponse(ApiErrorCode ErrorCode, string Message);
