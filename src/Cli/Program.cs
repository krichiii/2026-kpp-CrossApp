using Core.Abstractions;
using Core.Domain;

var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} {e.QuantityAfter}";
    Console.WriteLine(line);
    File.AppendAllText(logPath, line + Environment.NewLine);
}

ICatalogStore store = StoreFactory.Create(args);
var service = new CatalogService(store);

service.Changed += OnCatalogChanged;
int changes = 0;
service.Changed += (_, _) => changes++;

var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Receive(created.Id, 25);
Console.WriteLine($"Подій отримано: {changes}");
service.Changed -= OnCatalogChanged;
service.Receive(created.Id, 5); // тиха операція: підписка керована
Console.WriteLine($"Подій отримано: {changes}"); // +1 лише від лямбди-лічильника

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