using Core.Domain;
using Core.Abstractions;
public sealed class CatalogService(ICatalogStore store, INotificationSink? sink = null)
{
    private readonly ICatalogStore _store = store ?? throw new 
        ArgumentNullException(nameof(store));
    private readonly INotificationSink? _sink = sink;

    public event EventHandler<CatalogChangedEventArgs>? Changed;

    private void OnChanged(Product p, CatalogChangeKind kind, int delta)
    {
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity);
        _sink?.Notify(args);
        Changed?.Invoke(this, args);
    }

    public Product Add(string sku, string name, string unit, int quantity)
    {
        var product = Product.Create(Guid.NewGuid().ToString("N")[..8], sku, name, unit, quantity);
        _store.Add(product);
        OnChanged(product, CatalogChangeKind.Added, quantity); // після зміни, не до
        return product;
    }
    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.RegisterArrival(quantity); // якщо верхній рядок кине виняток – події не буде
        _store.Update(product);
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }
 // поруч із Find(string id) з тижня 5: тут критерій задає той, хто викликає
    public IReadOnlyList<Product> Find(Func<Product, bool> predicate) => _store.Find(predicate);
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) =>
        _store.List().Where(predicate).ToList();
    
    public IReadOnlyList<Product> All() => _store.List();
 }
