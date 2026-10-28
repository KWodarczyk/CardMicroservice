namespace CardDataApi.Models;

/// <summary>
/// Mirrors CardMicroserviceApi.Models.CardStatus. Kept as a flags enum so values are
/// represented consistently across the two services; the OpenAPI contract serializes
/// this as a string, so CardMicroserviceApi's generated client deserializes it by name.
/// </summary>
[Flags]
public enum CardStatus
{
    Ordered = 1 << 0,
    Inactive = 1 << 1,
    Active = 1 << 2,
    Restricted = 1 << 3,
    Blocked = 1 << 4,
    Expired = 1 << 5,
    Closed = 1 << 6
}
