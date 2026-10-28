namespace CardMicroserviceApi.Services;

/// <summary>Describes whether a rule's statuses require a particular PIN state.</summary>
internal enum PinCondition
{
    /// <summary>No PIN condition - the action is allowed regardless of PIN state.</summary>
    None,

    /// <summary>The action is allowed only when a PIN is set.</summary>
    RequiresPinSet,

    /// <summary>The action is allowed only when no PIN is set.</summary>
    RequiresNoPin
}
