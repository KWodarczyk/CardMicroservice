namespace CardDataApi.Models;

/// <summary>
/// Mirrors CardMicroserviceApi.Models.CardType. Kept as a flags enum so values are
/// represented consistently across the two services; the OpenAPI contract serializes
/// this as a string, so CardMicroserviceApi's generated client deserializes it by name.
/// </summary>
[Flags]
public enum CardType
{
    Prepaid = 1 << 0,
    Debit = 1 << 1,
    Credit = 1 << 2
}
