using System.Text.RegularExpressions;

namespace CardMicroserviceApi.Validation;

/// <summary>
/// Validates the shape of identifiers accepted by the card-actions API. These are
/// defense-in-depth checks (beyond simple null/empty checks) intended to reject
/// obviously malicious or malformed input before it reaches any downstream service
/// or gets written to logs: unbounded length (resource exhaustion / log flooding)
/// and control/whitespace/punctuation characters (log injection).
///
/// Note: card numbers here use the same alphanumeric id-like shape as userId rather
/// than a strict digits-only PAN format, because this project's sample/mock data
/// uses alphanumeric card numbers (e.g. "Card11"). A real banking system would
/// typically constrain card numbers to digits only (ISO/IEC 7812, 12-19 digits).
/// </summary>
public static partial class CardRequestValidator
{
    public const int MaxUserIdLength = 64;
    public const int MaxCardNumberLength = 32;

    // Allows letters, digits, underscore and hyphen only - typical shape for an
    // opaque account/card/user identifier. Rejects whitespace, control characters
    // and punctuation that have no legitimate use in an id and could enable log
    // injection (e.g. embedded CRLF forging fake log lines).
    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex IdPattern();

    public static bool IsValidUserId(string userId) =>
        userId.Length <= MaxUserIdLength && IdPattern().IsMatch(userId);

    public static bool IsValidCardNumber(string cardNumber) =>
        cardNumber.Length <= MaxCardNumberLength && IdPattern().IsMatch(cardNumber);
}

