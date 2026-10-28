using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.DTOs;

/// <summary>
/// Response returned by the actions-evaluation endpoint.
/// </summary>
public record CardActionsEvaluationResponse(
    CardType CardType,
    CardStatus CardStatus,
    bool IsPinSet,
    IReadOnlyList<CardAction> AllowedActions);
