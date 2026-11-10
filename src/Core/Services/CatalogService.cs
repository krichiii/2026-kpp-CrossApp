using System.Runtime.CompilerServices;
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
        if (quantity <= 0)
            throw new ArgumentException("Кількість приходу має бути більшою за нуль", nameof(quantity));
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

    public async IAsyncEnumerable<Product> ImportStreamAsync(
        ICatalogImporter importer, 
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default,
        IProgress<int>? progress = null)
    {
        ct.ThrowIfCancellationRequested();
        await foreach (var item in importer.LoadStreamAsync(path, ct, progress).WithCancellation(ct))
        {
            if (_store.GetById(item.Id) is not null) continue; // ідемпотентність імпорту
            _store.Add(item);
            OnChanged(item, CatalogChangeKind.Added, item.Quantity);
            yield return item;
        }
    }

    public async Task<int> ImportAsync(
        ICatalogImporter importer, 
        string path, 
        CancellationToken ct = default,
        IProgress<int>? progress = null)
    {
        ct.ThrowIfCancellationRequested();
        var items = await importer.LoadAsync(path, ct, progress);
        var added = 0;
        progress?.Report(0);

        for (var i = 0; i < items.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var item = items[i];
            if (_store.GetById(item.Id) is null)
            {
                _store.Add(item);
                OnChanged(item, CatalogChangeKind.Added, item.Quantity);
                added++;
            }

            var percent = items.Count > 0 ? (int)Math.Round((i + 1) * 100.0 / items.Count) : 100;
            progress?.Report(percent);
        }

        return added;
    }
 }
