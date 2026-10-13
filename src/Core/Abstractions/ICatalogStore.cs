using Core.Domain;
namespace Core.Abstractions;

public interface ICatalogStore
{
    IReadOnlyList<Product> List();
    Product? GetById(string id);
    IReadOnlyList<Product> Find(Func<Product, bool> predicate);
    void Add(Product item);
    void Update(Product item);
    bool Remove(string id);
 }
