using Core.Domain;

public static class ReportService
{
    // 1) фільтр + сортування + проєкція
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
        .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();

    // 2) топ-N зі стабільним порядком (використання власного методу TopBy)
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) => 
        source.TopBy(p => p.Quantity, n)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();
 
    // 3) групування з підрахунком і сумою (методний синтаксис)
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
        .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
        .OrderByDescending(s => s.TotalQuantity)
        .ToList();
/*
    // звіт у синтаксисі запитів (Query Syntax: from ... group ... into ...)
    public static IReadOnlyList<GroupSummary> ByUnitQuery(IEnumerable<Product> source) =>
        (from p in source
         group p by p.Unit into g
         let totalQty = g.Sum(x => x.Quantity)
         orderby totalQty descending
         select new GroupSummary(g.Key, g.Count(), totalQty))
        .ToList();
*/
    // 4) агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, list.Sum(p => p.Quantity),
                list.Average(p => p.Quantity), list.Max(p => p.Quantity));
    }

    // 5) GroupJoin двох колекцій (товари × переміщення) з проєкцією у зведений DTO
    public static IReadOnlyList<ProductMovementSummary> ProductMovementSummaries(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements) =>
        products.GroupJoin(
            movements,
            p => p.Id,
            m => m.ProductId,
            (p, movs) =>
            {
                var movList = movs.ToList();
                var incoming = movList.Where(m => m.Type == MovementType.Incoming).Sum(m => m.Quantity);
                var outgoing = movList.Where(m => m.Type == MovementType.Outgoing).Sum(m => m.Quantity);
                return new ProductMovementSummary(
                    p.Sku,
                    p.Name,
                    movList.Count,
                    incoming,
                    outgoing,
                    incoming - outgoing
                );
            })
            .OrderByDescending(s => s.TotalMovements)
            .ThenBy(s => s.Sku, StringComparer.Ordinal)
            .ToList();

    // 6) Join двох колекцій (товари × переміщення) з проєкцією у детальний DTO
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

    // 7) топ-N товарів за активністю переміщень
    public static IReadOnlyList<ProductMovementSummary> TopMovedProducts(
        IEnumerable<Product> products,
        IEnumerable<Movement> movements,
        int n) =>
        ProductMovementSummaries(products, movements)
            .TopBy(s => s.TotalMovements, n)
            .ToList();

    // 8) LowStock з проміжним ToList() у середині ланцюжка (для порівняння продуктивності)
    public static IReadOnlyList<StockLine> LowStockWithIntermediateToList(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
        .ToList() // передчасна матеріалізація у середині ланцюжка
        .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();

    // 9) TopByQuantity з проміжним ToList() перед Take (для порівняння продуктивності)
    public static IReadOnlyList<StockLine> TopByQuantityWithIntermediateToList(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity)
        .ToList() // передчасна матеріалізація 100 000 елементів перед Take
        .Take(n)
        .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
        .ToList();
}