using Core.Abstractions;
using Core.Domain;
using Core.Notifications;

var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "app.log");

// Ручний DI
INotificationSink sink = new CompositeNotificationSink(
    new ConsoleSink(),
    new FileSink(logPath)
);

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store, sink);

int changes = 0;
service.Changed += (_, _) => changes++;

var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");
service.Receive(created.Id, 5); 
Console.WriteLine($"Подій отримано: {changes}");

foreach (var p in service.All())
    Console.WriteLine($" {p.Id} {p.Sku,-10} {p.Name,-22} {p.Quantity,5} {p.Unit}");

try { service.Receive("unknown-id", 10); }
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[InvalidOperationException] {ex.Message}");
}
try { store.Add(Product.Create(created.Id, "SKU-999", "Дублікат товару", "шт", 10)); }
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[InvalidOperationException] {ex.Message}");
}

var inStock = service.Find(p => p.Quantity > 50);
Console.WriteLine("Товари з Quantity > 50:");
foreach (var p in inStock)
    Console.WriteLine($"  [{p.Sku}] {p.Name} — {p.Quantity} {p.Unit}");