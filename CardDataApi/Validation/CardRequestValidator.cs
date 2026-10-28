using System.Text.RegularExpressions;

namespace CardDataApi.Validation;

/// <summary>
/// Validates the shape of identifiers accepted by this backend's API. Mirrors
/// CardMicroserviceApi.Validation.CardRequestValidator so CardDataApi does not
/// rely solely on the façade for input hygiene (defense-in-depth: this API could
/// be called directly, e.g. from inside the same network/cluster).
/// </summary>
public static partial class CardRequestValidator
{
    public const int MaxUserIdLength = 64;
    public const int MaxCardNumberLength = 32;

    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex IdPattern();

    public static bool IsValidUserId(string userId) =>
        userId.Length <= MaxUserIdLength && IdPattern().IsMatch(userId);

    public static bool IsValidCardNumber(string cardNumber) =>
        cardNumber.Length <= MaxCardNumberLength && IdPattern().IsMatch(cardNumber);
}
