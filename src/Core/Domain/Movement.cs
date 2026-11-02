namespace Core.Domain;

public enum MovementType
{
    Incoming,
    Outgoing
}

public sealed record Movement(
    string Id,
    string ProductId,
    int Quantity,
    MovementType Type,
    DateTime Timestamp,
    string? Description = null
);

