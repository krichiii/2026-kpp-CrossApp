using Core.Abstractions;
using Core.Domain;

public sealed class CachingCatalogStore(ICatalogStore inner) : ICatalogStore
{
    private readonly ICatalogStore _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);

    public int CacheHits { get; private set; }
    public int CacheMisses { get; private set; }

    public Product? GetById(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        if (_cache.TryGetValue(id, out var cached))
        {
            CacheHits++;
            return cached;
        }

        CacheMisses++;
        var item = _inner.GetById(id);
        if (item is not null)
        {
            _cache[item.Id] = item;
        }

        return item;
    }

    public IReadOnlyList<Product> List()
    {
        var items = _inner.List();
        foreach (var item in items)
        {
            _cache[item.Id] = item;
        }

        return items;
    }

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Add(item);
        _cache[item.Id] = item;
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Update(item);
        _cache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var removed = _inner.Remove(id);
        if (removed)
        {
            _cache.Remove(id);
        }

        return removed;
    }
}

