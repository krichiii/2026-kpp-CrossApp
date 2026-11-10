using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Domain;
using Core.Dto;

public sealed class JsonCatalogImporter : ICatalogImporter
{
    public async IAsyncEnumerable<Product> LoadStreamAsync(
        string path, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        var index = 0;
        await foreach (var dto in JsonSerializer.DeserializeAsyncEnumerable<ProductDto>(stream, cancellationToken: ct))
        {
            index++;
            if (dto is null) continue;

            Product product;
            try
            {
                product = Product.FromDto(dto);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException($"Запис {index}: {ex.Message}", ex);
            }

            yield return product;
        }
    }

    public async Task<IReadOnlyList<Product>> LoadAsync(string path, CancellationToken ct = default)
    {
        var result = new List<Product>();
        await foreach (var product in LoadStreamAsync(path, ct).WithCancellation(ct))
        {
            result.Add(product);
        }
        return result;
    }
}
