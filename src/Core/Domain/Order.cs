namespace Core.Domain;

public sealed class Order
{
    public string Id { get; }
    public OrderStatus Status { get; private set; }

    private Order(string id, OrderStatus status)
    {
        Id = id;
        Status = status;
    }

    public static Order Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор замовлення обов'язковий", nameof(id));

        return new Order(id.Trim(), OrderStatus.Draft);
    }

    /// <summary>
    /// Перевірка допустимих переходів між станами через switch expression.
    /// Дозволені переходи:
    ///   Draft -> Confirmed
    ///   Draft -> Cancelled
    ///   Confirmed -> Cancelled
    /// Будь-які інші переходи (наприклад, із Cancelled або повторний перехід у той самий стан) є неприпустимими.
    /// </summary>
    public void ChangeStatus(OrderStatus newStatus)
    {
        Status = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) => OrderStatus.Confirmed,
            (OrderStatus.Draft, OrderStatus.Cancelled) => OrderStatus.Cancelled,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => OrderStatus.Cancelled,
            _ => throw new InvalidOperationException($"Неприпустимий перехід стану замовлення {Id}: з {Status} у {newStatus}")
        };
    }

    public void Confirm() => ChangeStatus(OrderStatus.Confirmed);
    public void Cancel() => ChangeStatus(OrderStatus.Cancelled);

    public override string ToString() => $"Замовлення {Id} [Стан: {Status}]";
}
