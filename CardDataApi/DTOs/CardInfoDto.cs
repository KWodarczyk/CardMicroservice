using CardDataApi.Models;

namespace CardDataApi.DTOs;

/// <summary>
/// Data-transfer-only shape describing a card over the wire (OpenAPI contract).
/// CardMicroserviceApi's generated client receives this shape and converts it
/// into its own internal CardDetails domain model.
/// </summary>
public record CardInfoDto(string CardNumber, CardType CardType, CardStatus CardStatus, bool IsPinSet);
