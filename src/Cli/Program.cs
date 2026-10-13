using Core.Abstractions;
using Core.Domain;

bool useFile = args.Contains("--file");
string dataDir = Directory.Exists("data") ? "data" : Path.Combine("..", "..", "data");
string dataPath = Path.Combine(dataDir, "catalog.json");
 
ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);
Console.WriteLine($"Сховище: {store.GetType().Name}");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);

service.Receive(created.Id, 25);
foreach (var p in service.All())
    Console.WriteLine($" {p.Id} {p.Sku,-10} {p.Name,-22} {p.Quantity,5} {p.Unit}");

Console.WriteLine();
Console.WriteLine("=== Сценарії відмови ===");

// 1. Спроба викликати Receive з неіснуючим id
try
{
    Console.WriteLine("Спроба зарахування товару з неіснуючим ID ('unknown-id')...");
    service.Receive("unknown-id", 10);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[InvalidOperationException] {ex.Message}");
}

// 2. Спроба додати товар з дублікатом id
try
{
    Console.WriteLine($"Спроба додати товар із дублікатом ID ('{created.Id}')...");
    store.Add(Product.Create(created.Id, "SKU-999", "Дублікат товару", "шт", 10));
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"[InvalidOperationException] {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("=== Демонстрація декоратора CachingCatalogStore (додадкове №1) ===");
var cachingStore = new CachingCatalogStore(store);

var p1 = cachingStore.GetById(created.Id);
Console.WriteLine($"Виклик 1: {p1?.Name} (Hits: {cachingStore.CacheHits}, Misses: {cachingStore.CacheMisses})");

var p2 = cachingStore.GetById(created.Id);
Console.WriteLine($"Виклик 2: {p2?.Name} (Hits: {cachingStore.CacheHits}, Misses: {cachingStore.CacheMisses})");

Console.WriteLine();
Console.WriteLine("=== Пошук з параметром Func<Product, bool> (додаткове №2) ===");

service.Add("SKU-102", "Патч-корд RJ-45", "шт", 15);

var inStock = service.Find(p => p.Quantity > 50);
Console.WriteLine("Товари з Quantity > 50:");
foreach (var p in inStock)
    Console.WriteLine($"  [{p.Sku}] {p.Name} — {p.Quantity} {p.Unit}");

var cables = service.Find(p => p.Name.Contains("Кабель", StringComparison.OrdinalIgnoreCase));
Console.WriteLine("Товари з назвою, що містить 'Кабель':");
foreach (var p in cables)
    Console.WriteLine($"  [{p.Sku}] {p.Name} — {p.Quantity} {p.Unit}");