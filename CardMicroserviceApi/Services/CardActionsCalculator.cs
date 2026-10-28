using CardMicroserviceApi.Models;

namespace CardMicroserviceApi.Services;

/// <summary>
/// Implements the "allowed actions" table. To add or change an action, add/update
/// its entry in <see cref="Rules"/> - no other code needs to change.
/// </summary>
public class CardActionsCalculator : ICardActionsCalculator
{
    private const CardType AllCardTypes = CardType.Prepaid | CardType.Debit | CardType.Credit;

    private const CardStatus AllCardStatuses =
        CardStatus.Ordered | CardStatus.Inactive | CardStatus.Active | CardStatus.Restricted
        | CardStatus.Blocked | CardStatus.Expired | CardStatus.Closed;

    /// <summary>Shared "every status, no PIN condition" map - reused by rules that allow all statuses unconditionally.</summary>
    private static readonly IReadOnlyDictionary<CardStatus, PinCondition> AllStatusesNoPinCondition =
        ToStatusDictionary(AllCardStatuses, PinCondition.None);

    private static readonly ActionRule[] Rules =
    [
        new ActionRule
        {
            ActionName = CardAction.ACTION1,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition> { [CardStatus.Active] = PinCondition.None }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION2,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition> { [CardStatus.Inactive] = PinCondition.None }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION3,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = AllStatusesNoPinCondition
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION4,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = AllStatusesNoPinCondition
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION5,
            AllowedCardTypes = CardType.Credit,
            AllowedStatuses = AllStatusesNoPinCondition
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION6,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Ordered] = PinCondition.RequiresPinSet,
                [CardStatus.Inactive] = PinCondition.RequiresPinSet,
                [CardStatus.Active] = PinCondition.RequiresPinSet,
                [CardStatus.Blocked] = PinCondition.RequiresPinSet
            }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION7,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Ordered] = PinCondition.RequiresNoPin,
                [CardStatus.Inactive] = PinCondition.RequiresNoPin,
                [CardStatus.Active] = PinCondition.RequiresNoPin,
                [CardStatus.Blocked] = PinCondition.RequiresPinSet
            }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION8,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Ordered] = PinCondition.None,
                [CardStatus.Inactive] = PinCondition.None,
                [CardStatus.Active] = PinCondition.None,
                [CardStatus.Blocked] = PinCondition.None
            }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION9,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = AllStatusesNoPinCondition
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION10,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Ordered] = PinCondition.None,
                [CardStatus.Inactive] = PinCondition.None,
                [CardStatus.Active] = PinCondition.None
            }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION11,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Inactive] = PinCondition.None,
                [CardStatus.Active] = PinCondition.None
            }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION12,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Ordered] = PinCondition.None,
                [CardStatus.Inactive] = PinCondition.None,
                [CardStatus.Active] = PinCondition.None
            }
        },
        new ActionRule
        {
            ActionName = CardAction.ACTION13,
            AllowedCardTypes = AllCardTypes,
            AllowedStatuses = new Dictionary<CardStatus, PinCondition>
            {
                [CardStatus.Ordered] = PinCondition.None,
                [CardStatus.Inactive] = PinCondition.None,
                [CardStatus.Active] = PinCondition.None
            }
        }
    ];

    /// <summary>Builds a status-to-PinCondition map for every individual status bit set in <paramref name="statuses"/>.</summary>
    private static IReadOnlyDictionary<CardStatus, PinCondition> ToStatusDictionary(CardStatus statuses, PinCondition condition)
    {
        var map = new Dictionary<CardStatus, PinCondition>();

        foreach (CardStatus status in Enum.GetValues<CardStatus>())
        {
            if (statuses.HasFlag(status))
            {
                map[status] = condition;
            }
        }

        return map;
    }

    public IReadOnlyList<CardAction> GetAllowedActions(CardType cardType, CardStatus cardStatus, bool isPinSet)
    {
        var allowedActions = new List<CardAction>();

        foreach (ActionRule rule in Rules)
        {
            if (!rule.AllowedCardTypes.HasFlag(cardType))
            {
                continue;
            }

            if (!rule.AllowedStatuses.TryGetValue(cardStatus, out PinCondition pinCondition))
            {
                continue;
            }

            bool pinConditionMet = pinCondition switch

            {
                PinCondition.RequiresPinSet => isPinSet,
                PinCondition.RequiresNoPin => !isPinSet,
                _ => true
            };

            if (pinConditionMet)
            {
                allowedActions.Add(rule.ActionName);
            }
        }

        return allowedActions;
    }
}
