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
        source.OrderByDescending(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
        .Take(n)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();
 
    // 3) групування з підрахунком і сумою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
        .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
        .OrderByDescending(s => s.TotalQuantity)
        .ToList();

    // 4) агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity),
                list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // 5) Join двох колекцій (товари × переміщення) з проєкцією у детальний DTO
    public static IReadOnlyList<ProductMovementDetail> ProductMovementDetails(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements) =>
        products.Join(
            movements,
            p => p.Id,
            m => m.ProductId,
            (p, m) => new ProductMovementDetail(
                m.Id,
                p.Sku,
                p.Name,
                m.Type,
                m.Quantity,
                m.Timestamp
            ))
            .OrderBy(d => d.Timestamp)
            .ToList();
}