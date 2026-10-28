using CardMicroserviceApi.Models;
using CardMicroserviceApi.Services;
using Xunit;

namespace CardMicroserviceApi.Tests.Services;

public class CardActionsCalculatorTests
{
    private readonly CardActionsCalculator _sut = new();

    public static IEnumerable<object[]> GetAllowedActionsCases()
    {
        // ACTION1: allowed only for Active status.
        yield return
        [
            CardType.Prepaid, CardStatus.Active, false,
            new[] { CardAction.ACTION1, CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION7, CardAction.ACTION8, CardAction.ACTION9, CardAction.ACTION10, CardAction.ACTION11, CardAction.ACTION12, CardAction.ACTION13 }
        ];
        yield return
        [
            CardType.Debit, CardStatus.Active, false,
            new[] { CardAction.ACTION1, CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION7, CardAction.ACTION8, CardAction.ACTION9, CardAction.ACTION10, CardAction.ACTION11, CardAction.ACTION12, CardAction.ACTION13 }
        ];

        // ACTION5: only applies to Credit cards.
        yield return
        [
            CardType.Credit, CardStatus.Active, false,
            new[] { CardAction.ACTION1, CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION5, CardAction.ACTION7, CardAction.ACTION8, CardAction.ACTION9, CardAction.ACTION10, CardAction.ACTION11, CardAction.ACTION12, CardAction.ACTION13 }
        ];

        // ACTION6/ACTION7: require a PIN set / no PIN set respectively, for Ordered/Inactive/Active statuses.
        yield return
        [
            CardType.Debit, CardStatus.Ordered, true,
            new[] { CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION6, CardAction.ACTION8, CardAction.ACTION9, CardAction.ACTION10, CardAction.ACTION12, CardAction.ACTION13 }
        ];
        yield return
        [
            CardType.Debit, CardStatus.Ordered, false,
            new[] { CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION7, CardAction.ACTION8, CardAction.ACTION9, CardAction.ACTION10, CardAction.ACTION12, CardAction.ACTION13 }
        ];

        // ACTION8: allowed for Ordered/Inactive/Active/Blocked. Example from the specification:
        // CREDIT + BLOCKED, with PIN set -> ACTION3, ACTION4, ACTION5, ACTION6, ACTION7, ACTION8, ACTION9.
        yield return
        [
            CardType.Credit, CardStatus.Blocked, true,
            new[] { CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION5, CardAction.ACTION6, CardAction.ACTION7, CardAction.ACTION8, CardAction.ACTION9 }
        ];

        // Same as above but without PIN set -> ACTION6/ACTION7 are no longer allowed.
        yield return
        [
            CardType.Credit, CardStatus.Blocked, false,
            new[] { CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION5, CardAction.ACTION8, CardAction.ACTION9 }
        ];

        // ACTION9: allowed for every status. Example from the specification:
        // PREPAID + CLOSED -> ACTION3, ACTION4, ACTION9.
        yield return
        [
            CardType.Prepaid, CardStatus.Closed, false,
            new[] { CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION9 }
        ];

        // RESTRICTED: per the table, only allows ACTION3, ACTION4, ACTION9 (and ACTION5, but only for Credit cards).
        yield return
        [
            CardType.Debit, CardStatus.Restricted, true,
            new[] { CardAction.ACTION3, CardAction.ACTION4, CardAction.ACTION9 }
        ];
    }

    [Theory]
    [MemberData(nameof(GetAllowedActionsCases))]
    public void GetAllowedActions_ReturnsExpectedActions(CardType cardType, CardStatus cardStatus, bool isPinSet, CardAction[] expected)
    {
        var result = _sut.GetAllowedActions(cardType, cardStatus, isPinSet);

        Assert.Equal(expected, result);
    }
}
