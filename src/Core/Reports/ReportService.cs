using Core.Domain;

public static class ReportService
{
    // 1) фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
        .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();

    // 2) топ-N зі стабільним порядком
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) => 
        source.TopBy(p => p.Quantity, n)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();
}