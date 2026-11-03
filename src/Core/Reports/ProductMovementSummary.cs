public sealed record ProductMovementSummary(
    string Sku,
    string Name,
    int TotalMovements,
    int IncomingQuantity,
    int OutgoingQuantity,
    int NetChange
);

