using Core.Domain;

public interface ICatalogImporter
{
    Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default, IProgress<int>? progress = null);
    IAsyncEnumerable<Product> LoadStreamAsync(string path, CancellationToken ct = default, IProgress<int>? progress = null);
}
