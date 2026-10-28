using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.DTOs;

/// <summary>
/// Request body for directly evaluating allowed actions from explicit card
/// attributes, bypassing card lookup. Useful for testing/demonstrating the
/// rule engine without needing an existing user/card record.
/// </summary>
public record CardActionsEvaluationRequest(CardType CardType, CardStatus CardStatus, bool IsPinSet);
