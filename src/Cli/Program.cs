using Core.Abstractions;
using Core.Domain;
using Core.Notifications;

var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "app.log");

INotificationSink sink = new CompositeNotificationSink(
    new ConsoleSink(),
    new FileSink(logPath)
);

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store, sink);

var source = service.All().ToList();
var query = source.Where(p => p.Quantity < 10); // запит побудований, але не виконаний
 
Console.WriteLine($"Дефіцитних до змін: {query.Count()}"); // ось тут запит виконується
source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3));
Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // додано новий елемент і число вже інше

var snapshot = query.ToList(); // результат зафіксовано
source.Clear();

Console.WriteLine($"У знімку лишилось: {snapshot.Count}");

var items = service.All();
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top": Print(ReportService.TopByQuantity(items, 5)); break;

    case "report-low": Print(ReportService.LowStock(items, 10)); break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($" {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;

    default: Console.WriteLine("report-top | report-low | report-groups | report-sum"); break;
 }

 static void Print(IReadOnlyList<StockLine> lines)
 {
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
 }