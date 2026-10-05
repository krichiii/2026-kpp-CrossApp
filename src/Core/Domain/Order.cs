namespace Core.Domain;

public sealed record OrderItem(string ProductId, string Sku, int Quantity);

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    public string Id { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items;

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

    public void AddItem(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        // Інваріант з боку Order: можна редагувати лише у стані Draft
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                $"Не можна додавати товари до замовлення {Id} у стані {Status}");

        // Власний інваріант: кількість позитивна
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Кількість товару в замовленні має бути більшою за нуль");

        // Інваріант з боку Product: не більше доступного залишку
        if (quantity > product.Quantity)
            throw new InvalidOperationException(
                $"Не можна замовити {quantity} од. '{product.Name}' [{product.Sku}]: доступно лише {product.Quantity}");

        _items.Add(new OrderItem(product.Id, product.Sku, quantity));
    }

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

    public override string ToString() => $"Замовлення {Id} [Стан: {Status}, позицій: {_items.Count}]";
}
