using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.DTOs;

/// <summary>
/// Response returned by the allowed-actions endpoint.
/// </summary>
public record CardActionsResponse(string UserId, string CardNumber, IReadOnlyList<CardAction> AllowedActions);
