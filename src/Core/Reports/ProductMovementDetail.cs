using Core.Domain;

public sealed record ProductMovementDetail(
    string MovementId,
    string Sku,
    string Name,
    MovementType Type,
    int Quantity,
    DateTime Timestamp
);

