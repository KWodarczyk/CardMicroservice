namespace CardMicroserviceApi.Services;

/// <summary>
/// Error codes an <see cref="ICardService"/> implementation can report instead of
/// returning null, allowing callers (e.g. CardActionsController) to map each failure
/// to a specific HTTP response.
/// </summary>
public enum CardServiceErrorCode
{
    /// <summary>The card (or user) does not exist.</summary>
    CardNotFound,

    /// <summary>The backend card data service could not be reached or timed out.</summary>
    BackendUnavailable,

    /// <summary>An unexpected error occurred while retrieving the card data.</summary>
    UnexpectedError,
}
